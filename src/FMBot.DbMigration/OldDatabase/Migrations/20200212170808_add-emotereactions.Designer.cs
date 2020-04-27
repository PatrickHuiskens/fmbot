﻿// <auto-generated />

using System;
using FMBot.Persistence.EntityFrameWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FMBot.DbMigration.OldDatabase.Migrations
{
    [DbContext(typeof(FMBotDbContext))]
    [Migration("20200212170808_add-emotereactions")]
    partial class addemotereactions
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FMBot.Data.Entities.Friend", b =>
                {
                    b.Property<int>("FriendID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("FriendID")
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("FriendUserID")
                        .HasColumnName("FriendUserID")
                        .HasColumnType("int");

                    b.Property<string>("LastFMUserName")
                        .HasColumnName("LastFMUserName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserID")
                        .HasColumnName("UserID")
                        .HasColumnType("int");

                    b.HasKey("FriendID")
                        .HasName("PK_dbo.Friends");

                    b.HasIndex("FriendUserID")
                        .HasName("IX_FriendUserID");

                    b.HasIndex("UserID")
                        .HasName("IX_UserID");

                    b.ToTable("Friends");
                });

            modelBuilder.Entity("FMBot.Data.Entities.Guild", b =>
                {
                    b.Property<int>("GuildID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("GuildID")
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool?>("Blacklisted")
                        .HasColumnType("bit");

                    b.Property<int>("ChartTimePeriod")
                        .HasColumnType("int");

                    b.Property<int>("FmEmbedType")
                        .HasColumnType("int");

                    b.Property<string>("DiscordGuildID")
                        .HasColumnName("DiscordGuildID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmoteReactions")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("TitlesEnabled")
                        .HasColumnType("bit");

                    b.HasKey("GuildID")
                        .HasName("PK_dbo.Guilds");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("FMBot.Data.Entities.GuildUsers", b =>
                {
                    b.Property<int>("GuildID")
                        .HasColumnName("GuildID")
                        .HasColumnType("int");

                    b.Property<int>("UserID")
                        .HasColumnName("UserID")
                        .HasColumnType("int");

                    b.HasKey("GuildID", "UserID")
                        .HasName("PK_dbo.GuildUsers");

                    b.HasIndex("GuildID")
                        .HasName("IX_GuildID");

                    b.HasIndex("UserID")
                        .HasName("IX_UserID");

                    b.ToTable("GuildUsers");
                });

            modelBuilder.Entity("FMBot.Data.Entities.User", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("UserID")
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool?>("Blacklisted")
                        .HasColumnType("bit");

                    b.Property<int>("ChartTimePeriod")
                        .HasColumnType("int");

                    b.Property<int>("FmEmbedType")
                        .HasColumnType("int");

                    b.Property<string>("DiscordUserID")
                        .HasColumnName("DiscordUserID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("Featured")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastGeneratedChartDateTimeUtc")
                        .HasColumnType("datetime");

                    b.Property<bool?>("TitlesEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserNameLastFM")
                        .HasColumnName("UserNameLastFM")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserType")
                        .HasColumnType("int");

                    b.HasKey("UserID")
                        .HasName("PK_dbo.Users");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FMBot.Data.Entities.Friend", b =>
                {
                    b.HasOne("FMBot.Data.Entities.User", "FriendUser")
                        .WithMany("FriendsFriendUser")
                        .HasForeignKey("FriendUserID")
                        .HasConstraintName("FK_dbo.Friends_dbo.Users_FriendUserID");

                    b.HasOne("FMBot.Data.Entities.User", "User")
                        .WithMany("FriendsUser")
                        .HasForeignKey("UserID")
                        .HasConstraintName("FK_dbo.Friends_dbo.Users_UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FMBot.Data.Entities.GuildUsers", b =>
                {
                    b.HasOne("FMBot.Data.Entities.Guild", "Guild")
                        .WithMany("GuildUsers")
                        .HasForeignKey("GuildID")
                        .HasConstraintName("FK_dbo.GuildUsers_dbo.Guilds_GuildID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FMBot.Data.Entities.User", "User")
                        .WithMany("GuildUsers")
                        .HasForeignKey("UserID")
                        .HasConstraintName("FK_dbo.GuildUsers_dbo.Users_UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
