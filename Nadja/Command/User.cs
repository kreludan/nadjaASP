﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Nadja.Models;

namespace Nadja.Command
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("profile"), RequireContext(ContextType.Guild)]
        public async Task ProfileAsync()
        {
            Dal.DoConnection();
            ServerUser serverUser = Dal.GetServerUser(Context.User.Id.ToString(), Context.Guild.Id.ToString());


            EmbedBuilder builder = new EmbedBuilder();

            Construct(builder, serverUser, Context.User.Id.ToString());
            Dal.CloseConnection();

            await ReplyAsync("", false, builder.Build());
        }

        [Command("profile"), RequireContext(ContextType.Guild)]
        public async Task ProfilePlayerAsync([Remainder] string name)
        {
            Dal.DoConnection();
            name = Helper.DiscordPingDelimiter(name);

            string idUser = Dal.GetIdUser(name);

            EmbedBuilder builder = new EmbedBuilder();
            ServerUser serverUser = null;

            if (idUser != null)
                serverUser = Dal.GetServerUser(idUser, Context.Guild.ToString());
            else
                serverUser = Dal.GetServerUser(name, Context.Guild.ToString());

            Construct(builder, serverUser, idUser, false);
            Dal.CloseConnection();

            await ReplyAsync("", false, builder.Build());
        }

        [Command("p"), RequireContext(ContextType.Guild)]
        public async Task PrAsync()
        {
            string idUser = Context.User.Id.ToString();
            Dal.DoConnection();
            ServerUser serverUser = Dal.GetServerUser(idUser, Context.Guild.Id.ToString());
            EmbedBuilder builder = new EmbedBuilder();

            if (serverUser == null)
            {
                User user = Dal.GetUser(idUser);
                if (user == null)
                {
                    builder.AddField("This user does not exists", "No game played")
                        .WithColor(Color.DarkerGrey);
                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    serverUser = new ServerUser(user);
                }

            }

            if(builder.Fields.Count <= 0)
            {
                List<ServerUser> everyServerUsers = Dal.GetEveryUser(Context.Guild.Id.ToString());
                builder.WithTitle($"Profile of {serverUser.DiscordName}");
                builder.AddField($"Rank : {Helper.GetRank(everyServerUsers, serverUser)}", $"Points : {serverUser.Points}", true);
                builder.AddField($"Searches : {serverUser.GetTotalSearchs()}", $"Legendaries found : {serverUser.CountLegendaries()}", true);
                builder.WithFooter($"Luck coefficient : {serverUser.GetLuck()}");
                builder.WithColor(Color.DarkOrange);
            }
            Dal.CloseConnection();

            await ReplyAsync("", false, builder.Build());
        }





        [Command("ranks"), RequireContext(ContextType.Guild)]
        public async Task RanksAsync()
        {
            Dal.DoConnection();
            EmbedBuilder builder = new EmbedBuilder();
            List<ServerUser> listRanks = Dal.GetEveryUser(Context.Guild.Id.ToString());

            listRanks = Helper.GetRanking(listRanks);
            builder.WithTitle($"TOP 10 PLAYERS ON { Context.Guild.Name} \n ");
            string aString = "";
            for (int i = 0; i < listRanks.Count; i++)
            {
                aString += Helper.GetRank(i + 1) + " : " + listRanks[i].ServerNameUser + "\n";
            }
            Dal.CloseConnection();

            builder.WithDescription(aString);

            builder.WithColor(Color.DarkTeal);
            await ReplyAsync("", false, builder.Build());
        }

        private void Construct(EmbedBuilder builder, ServerUser serverUser, string idUser, bool ownProfile = true)
        {
            if (serverUser == null)
            {
                User user = Dal.GetUser(idUser);
                if(user == null)
                {
                    builder.AddField("This user does not exists", "No game played")
                        .WithColor(Color.DarkerGrey);
                    return;
                } else
                {
                    serverUser = new ServerUser(user);
                }
                

            }
            else
            {
                if (ownProfile)
                    builder.WithImageUrl(Context.User.GetAvatarUrl());
            }
            List<ServerUser> everyServerUsers = Dal.GetEveryUser(Context.Guild.Id.ToString());

            string rank = Helper.GetRank(everyServerUsers, serverUser);
            
            if (serverUser.Points != 0)
            {
                builder.WithTitle($"Profile of {serverUser.DiscordName}")
                    .AddField("Rank", $"{rank}", true)
                    .AddField("Score", $"{serverUser.Points}", true);
            }
            else
            {
                builder.WithTitle($"Profile of {serverUser.DiscordName}")
                     .AddField("Rank", "Never played", true)
                     .AddField("Score", "Never played", true);
            }
            builder.AddField("Gems", $":gem: {serverUser.Gems}", false);
            
            if (serverUser.GetTotalSearchs() > 0)
            {
                builder.AddField("Items found :",
                    ":black_heart: Common items : " + serverUser.Common +
                    "\n:green_heart: Uncommon items : " + serverUser.Uncommon +
                    "\n:blue_heart: Rare items : " + serverUser.Rare +
                    "\n:purple_heart: Epic items : " + serverUser.Epic +
                    "\n:yellow_heart: Legendary items : " + serverUser.CountLegendaries(), true);
                
                string display = "";
                if (serverUser.CountLegendaries() <= 0)
                    display = "No legendary item...";
                else
                {
                    for (int i = 0; i < serverUser.CountLegendaries(); i++)
                        display += serverUser.Legendaries[i].Name + " \n";
                }
                
                builder.AddField("Legendary items :", display, true);

                double luckCoeff = serverUser.GetLuck();
                builder.WithFooter($"Luck coefficient : {luckCoeff}");
            }
            
            builder.WithColor(Color.Gold);
        }


    }
}
