﻿using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System.Threading.Tasks;
using static FMBot.Bot.FMBotUtil;

namespace FMBot.Services
{
    internal class SpotifyService
    {
        public static JsonCfg.ConfigJson cfgjson = JsonCfg.GetJSONData();

        public async Task<SearchItem> GetSearchResultAsync(string searchValue, SearchType searchType = SearchType.Track)
        {
            //Create the auth object
            ClientCredentialsAuth auth = new ClientCredentialsAuth()
            {
                ClientId = cfgjson.SpotifyKey,
                ClientSecret = cfgjson.SpotifySecret,
                Scope = Scope.None,
            };
            Token token = auth.DoAuth();

            SpotifyWebAPI spotify = new SpotifyWebAPI()
            {
                TokenType = token.TokenType,
                AccessToken = token.AccessToken,
                UseAuth = true
            };
            SearchItem result = await spotify.SearchItemsAsync(searchValue, SearchType.Track);

            return result;
        }
    }
}
