using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using FMBot.Bot.Configurations;
using FMBot.Bot.Extensions;
using FMBot.Bot.Interfaces;
using FMBot.Bot.Models;
using FMBot.Bot.Resources;
using FMBot.Persistence.Domain.Models;
using FMBot.Persistence.EntityFrameWork;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PostgreSQLCopyHelper;

namespace FMBot.Bot.Services
{
    public class IndexService : IIndexService
    {
        private readonly LastfmClient _lastFMClient = new LastfmClient(ConfigData.Data.LastFm.Key, ConfigData.Data.LastFm.Secret);

        private readonly IUserIndexQueue _userIndexQueue;

        public IndexService(IUserIndexQueue userIndexQueue)
        {
            this._userIndexQueue = userIndexQueue;
            this._userIndexQueue.UsersToIndex.SubscribeAsync(OnNextAsync);
        }

        private async Task OnNextAsync(User obj)
        {
            await StoreArtistsForUser(obj);
        }

        public void IndexGuild(IReadOnlyList<User> users)
        {
            Console.WriteLine($"Starting artist update for {users.Count} users");

            this._userIndexQueue.Publish(users.ToList());
        }

        private async Task StoreArtistsForUser(User user)
        {
            Thread.Sleep(1300);

            Console.WriteLine($"Starting artist store for {user.UserNameLastFM}");

            var topArtists = new List<LastArtist>();

            var amountOfPages = Constants.ArtistsToIndex / 1000;
            for (int i = 1; i < amountOfPages + 1; i++)
            {
                var artistResult = await this._lastFMClient.User.GetTopArtists(user.UserNameLastFM,
                    LastStatsTimeSpan.Overall, i, 1000);

                topArtists.AddRange(artistResult);

                if (artistResult.Count() < 1000)
                {
                    break;
                }

                Statistics.LastfmApiCalls.Inc();
            }

            if (topArtists.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var artists = topArtists.Select(a => new UserArtist
            {
                LastUpdated = now,
                Name = a.Name,
                Playcount = a.PlayCount.Value,
                UserId = user.UserId
            }).ToList();

            await InsertArtistsIntoDatabase(artists, user.UserId, now);
        }

        private static async Task InsertArtistsIntoDatabase(IReadOnlyList<UserArtist> artists, int userId, DateTime now)
        {
            await using var db = new FMBotDbContext(ConfigData.Data.Database.ConnectionString);
            var connString = db.Database.GetDbConnection().ConnectionString;
            var copyHelper = new PostgreSQLCopyHelper<UserArtist>("public", "user_artists")
                .MapText("name", x => x.Name)
                .MapInteger("user_id", x => x.UserId)
                .MapInteger("playcount", x => x.Playcount)
                .MapTimeStamp("last_updated", x => x.LastUpdated);

            await using var connection = new NpgsqlConnection(connString);
            connection.Open();

            await using var deleteCurrentArtists = new NpgsqlCommand($"DELETE FROM public.user_artists WHERE user_id = {userId};", connection);
            await deleteCurrentArtists.ExecuteNonQueryAsync().ConfigureAwait(false);

            await copyHelper.SaveAllAsync(connection, artists).ConfigureAwait(false);

            await using var setIndexTime = new NpgsqlCommand($"UPDATE public.users SET last_indexed='{now:u}' WHERE user_id = {userId};", connection);
            await setIndexTime.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task StoreGuildUsers(IGuild guild, IReadOnlyCollection<IGuildUser> guildUsers)
        {
            var userIds = guildUsers.Select(s => s.Id).ToList();

            await using var db = new FMBotDbContext(ConfigData.Data.Database.ConnectionString);
            var existingGuild = await db.Guilds
                .Include(i => i.GuildUsers)
                .FirstAsync(f => f.DiscordGuildId == guild.Id);

            var users = await db.Users
                .Include(i => i.Artists)
                .Where(w => userIds.Contains(w.DiscordUserId))
                .Select(s => new GuildUser
                {
                    GuildId = existingGuild.GuildId,
                    UserId = s.UserId
                })
                .ToListAsync();

            var connString = db.Database.GetDbConnection().ConnectionString;
            var copyHelper = new PostgreSQLCopyHelper<GuildUser>("public", "guild_users")
                .MapInteger("guild_id", x => x.GuildId)
                .MapInteger("user_id", x => x.UserId);

            await using var connection = new NpgsqlConnection(connString);
            connection.Open();

            await using var deleteCurrentArtists = new NpgsqlCommand($"DELETE FROM public.guild_users WHERE guild_id = {existingGuild.GuildId};", connection);
            await deleteCurrentArtists.ExecuteNonQueryAsync().ConfigureAwait(false);

            await copyHelper.SaveAllAsync(connection, users).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<User>> GetUsersToIndex(IReadOnlyCollection<IGuildUser> guildUsers)
        {
            var userIds = guildUsers.Select(s => s.Id).ToList();

            var tooRecent = DateTime.UtcNow.Add(-Constants.GuildIndexCooldown);

            await using var db = new FMBotDbContext(ConfigData.Data.Database.ConnectionString);
            return await db.Users
                .Include(i => i.Artists)
                .Where(w => userIds.Contains(w.DiscordUserId)
                && (w.LastIndexed == null || w.LastIndexed <= tooRecent))
                .ToListAsync();
        }

        public async Task<int> GetIndexedUsersCount(IReadOnlyCollection<IGuildUser> guildUsers)
        {
            var userIds = guildUsers.Select(s => s.Id).ToList();

            var indexCooldown = DateTime.UtcNow.Add(-Constants.GuildIndexCooldown);

            await using var db = new FMBotDbContext(ConfigData.Data.Database.ConnectionString);
            return await db.Users
                .AsQueryable()
                .Where(w => userIds.Contains(w.DiscordUserId)
                    && w.LastIndexed != null && w.LastIndexed >= indexCooldown)
                .CountAsync();
        }
    }
}
