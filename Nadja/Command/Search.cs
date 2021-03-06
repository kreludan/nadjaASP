﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static Stopwatch stopwatch = new Stopwatch();
        private static List<Search> searches = new List<Search>();
        private readonly int cooldownTime = 30000;



        //-search command
        [Command("search")]
        [Alias("s")]
        public async Task SearchAsync()
        {

            if (!stopwatch.IsRunning)
                stopwatch.Start();

            string idUser = Context.User.Id.ToString();
            string nameUser = Context.User.Username;

            bool valid = true;

            foreach(Search search in searches)
            {
                if(search.DiscordID == idUser)
                {
                    if(stopwatch.ElapsedMilliseconds >= search.Time + cooldownTime)
                    {
                        valid = true;
                        searches.Remove(search);
                        break;
                    } else
                    {
                        valid = false;
                        await ReplyAsync($"**{Context.User.Username}**, please wait **{(int)((cooldownTime - (stopwatch.ElapsedMilliseconds - search.Time)) / 1000 + 1)}** seconds before using this command again.", false);
                    }
                }
            }

            if(valid)
            {
                searches.Add(new Search(idUser, stopwatch.ElapsedMilliseconds));

                Dal.DoConnection();
                User user = Dal.GetUser(idUser);
                if (user == null)
                {
                    Dal.CreateUser(idUser, nameUser);
                    user = Dal.GetUser(idUser);
                }

                double beforeLuck = user.GetLuck();
                EmbedBuilder builder = new EmbedBuilder();
                int dice = Helper.rng.Next(1, 1001); // 0.1% to loot a legendary item
                List<Legendary> legendariesList = Dal.GetEveryLegendaries().ToList();

                user.LastTimeSearch = Helper.GetCurrentTime();

                double luckAfterInactivity = user.GetLuck();

                string title = "";

                if (dice <= 1)
                {
                    int itemFound = Helper.rng.Next(0, legendariesList.Count);
                    Legendary legendary = legendariesList[itemFound];
                    title = $":clap: :yellow_heart: :clap: {Context.User.Username} just found {legendary.Name} !!! A legendary item !!!!! :clap: :yellow_heart: :clap:";
                    builder.WithColor(Color.Gold);

                    Dal.AddLegendary(user, legendary);
                }
                else
                {
                    Location location = Dal.GetLocationFromInt(Helper.rng.Next(1, 22));
                    int totalItemAmount = location.GetTotalItemsInArea();
                    int idItemSelected = Helper.rng.Next(0, totalItemAmount);

                    List<Item> everyItems = location.GetEveryItems();
                    int amount = Helper.rng.Next(location.GetEveryItems().Count);



                    Item itemSelected = everyItems[amount];

                    if (epicItems.Contains(itemSelected.Name))
                    {
                        title = $":purple_heart:  {Context.User.Username} just found {itemSelected.Name} in {location.Name} !!! An epic item !!! :purple_heart: ";
                        builder.WithColor(Color.Purple);
                        Helper.AddItemFound(Helper.Rarity.Epic, user);
                    }
                    else
                    {
                        if (rareItems.Contains(itemSelected.Name))
                        {
                            title = $":blue_heart: {Context.User.Username} just found {itemSelected.Name} in {location.Name} !! A rare item !! :blue_heart:";
                            builder.WithColor(Color.Blue);
                            Helper.AddItemFound(Helper.Rarity.Rare, user);
                        }
                        else
                        {
                            if (uncommonItems.Contains(itemSelected.Name))
                            {
                                title = $":green_heart: {Context.User.Username} just found {itemSelected.Name} in {location.Name} ! An uncommon item :green_heart:";
                                builder.WithColor(Color.Green);
                                Helper.AddItemFound(Helper.Rarity.Uncommon, user);
                            }
                            else
                            {
                                title = $"{Context.User.Username} just found {itemSelected.Name} in {location.Name} ...";
                                builder.WithColor(Color.LightGrey);
                                Helper.AddItemFound(Helper.Rarity.Common, user);
                            }
                        }
                    }

                }


                List<double> allSearches = Dal.GetEverySearches();
                int totalSearches = (int)allSearches.Sum();
                double afterLuck = user.GetLuck();
                double luckModifierSearch = Math.Round(afterLuck - luckAfterInactivity, 6);
                string footer = "";
                double final = Math.Round(afterLuck - beforeLuck, 6);

                if (final > 0)
                    footer += "+";

                footer += $"**{final}** luck" + Environment.NewLine +
                    $"*+{Math.Round(luckAfterInactivity - beforeLuck, 6)} from inactivity regained" + Environment.NewLine;

                if (luckModifierSearch > 0)
                    footer += "+";

                footer += $"{luckModifierSearch} from the search*";
                

                if (totalSearches % 100 == 0)
                {
                    int totalLegendaries = Dal.GetEveryPossess();
                    footer += Environment.NewLine + $"{totalSearches}th search! {totalLegendaries} legendaries found in total."; // May break
                }

                builder.AddField(title, footer);
                await ReplyAsync(embed: builder.Build());


                Dal.CloseConnection();



            }

        }

        [Command("loot")]
        public async Task LootDefaultAsync()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.AddField("To use command loot :", "-loot <location> [1 < quantity (default 10) < 25] [0 < rare item % (default 1) < 100]");
            builder.WithColor(Color.DarkRed);
            await ReplyAsync(embed: builder.Build());

        }

        [Command("loot")]
        public async Task LootDefaultAsync(string place)
        {
            Dal.DoConnection();
            Location location = Dal.GetLocation(place);
            EmbedBuilder builder = new EmbedBuilder();
            Dal.CloseConnection();

            if (location != null)
                DoLoot(builder, location);
            else
                builder.WithTitle("Check the name of your location.");

            await ReplyAsync(embed: builder.Build());
        }

        [Command("loot")]
        public async Task LootQtyAsync(string place, int qty)
        {
            Dal.DoConnection();
            Location location = Dal.GetLocation(place);
            Dal.CloseConnection();

            EmbedBuilder builder = new EmbedBuilder();

            if (location != null)
            {
                if (qty > 0 && qty <= 25)
                    DoLoot(builder, location, qty);
                else
                    builder.WithTitle("Quantity have to be between 1 and 25.");

            }
            else
                builder.WithTitle("Check the name of your location.");

            await ReplyAsync(embed: builder.Build());
        }

        [Command("loot")]
        public async Task LootQtyRtyAsync(string place, int qty, float rare)
        {
            Dal.DoConnection();
            Location location = Dal.GetLocation(place);
            EmbedBuilder builder = new EmbedBuilder();
            Dal.CloseConnection();

            if (location != null)
            {
                if (qty > 0 && qty <= 25)
                    if (rare >= 0 && rare <= 100)
                        DoLoot(builder, location, qty, rare);
                    else
                        builder.WithTitle("Rarity have to be between 0 and 100.");
                else
                    builder.WithTitle("Quantity have to be between 1 and 25.");
            }
            else
            {
                builder.WithTitle("Check the name of your location.");
            }

            await ReplyAsync(embed: builder.Build());
        }

        public void DoLoot(EmbedBuilder builder, Location location, int quantity = 10, float rarity = 1)
        {
            string items = "";
            List<int> itemAlreadyPicked = new List<int>();
            for (int i = 0; i < quantity; i++)
            {

                if (Helper.rng.NextDouble() * 100 <= 100 - rarity)
                {
                    int idItemSelected = Helper.rng.Next(1, location.GetTotalItemsInArea() + 1);
                    while (itemAlreadyPicked.Contains(idItemSelected))
                    {
                        idItemSelected = Helper.rng.Next(1, location.GetTotalItemsInArea() + 1);
                    }

                    Item itemSelected = new Item();

                    int totalItemAmount = location.GetTotalItemsInArea();

                    List<Item> everyItems = location.GetEveryItems();
                    int amount = Helper.rng.Next(location.GetEveryItems().Count);

                    itemSelected = everyItems[amount];

                    itemAlreadyPicked.Add(idItemSelected);

                    items += itemSelected.Name + Environment.NewLine;

                }
                else
                    items += randomItems[Helper.rng.Next(0, randomItems.Count)] + " \n";
            }

            
            builder.AddField($"{quantity} items in {location.Name} (Rare item : {rarity}%)", items);
            
            builder.WithColor(Color.DarkRed);
        }


        private List<string> randomItems = new List<string> { "Tree of Life", "Arcane Stone", "Holy Blood", "Mithril", "Jewel Sword", "Meteorite", "Muramasa", "Ogre Skin", "Water", "Bullets", "Tights", "Cookie", "Arrows", "Heartbeat Sensor" };

        private List<string> legendaryItems = new List<string> { "Tree of Life", "Arcane Stone", "Holy Blood", "Mithril", "Jewel Sword", "Meteorite", "Muramasa", "Ogre Skin", "Heartbeat Sensor" };

        private List<string> epicItems = new List<string> { "Holy Grail", "Masamune", "Fresh Sashimi", "Burdock", "Sweet Potato", "Soy Sauce", "Laptop (broken screen)", "Anatomy Model", "Playing Cards" };

        private List<string> rareItems = new List<string>{"Tuna", "Ramen", "Garlic", "Potato", "Fountain Pen", "Flower", "Holy Water", "Stallion Medal", "Bacchus",
            "Cooking Pot", "Kitchen Knife", "Glass Cup", "Carp", "Mudfish", "Saury" , "Cross", "Cassock"};

        private List<string> uncommonItems = new List<string>{ "Lighter", "Honey", "Ice", "Pill", "Alcohol", "Coffee", "Orange", "Chocolate", "Thick Paper",
            "TV", "Cookie",  "Curry Powder", "Fabric Armor", "Glass Cup", "Ripped Scroll - 1", "Ripped Scroll - 2",  "Buddhist Scripture", "Wooden Fish", "Whetstone", "IM-10"};



        [Command("luck")]
        [Alias("l")]
        public async Task LuckAsync()
        {
            Dal.DoConnection();
            User user = Dal.GetUser(Context.User.Id.ToString());
            if(user != null)
            {
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithTitle($"{Context.User.Username}, your luck coefficient is {user.GetLuck()}")
                                .WithColor(Color.DarkGreen);
                await ReplyAsync("", false, builder.Build());
            }
            Dal.CloseConnection();

        }

        [Command("luck")]
        [Alias("l")]
        public async Task LuckPlayerAsync(IGuildUser guildUser)
        {
            EmbedBuilder builder = new EmbedBuilder();

            Dal.DoConnection();
            User user = Dal.GetUser(guildUser.Id.ToString());
            Dal.CloseConnection();

            if (user == null)
            {
                builder.WithTitle($"The luck of {guildUser.Username} is {user.GetLuck()}")
                .WithColor(Color.DarkGreen);
            }
            else
            {
                builder.WithTitle($"{guildUser.Username} doesn't exists...")
                    .WithColor(Color.DarkGreen);

            }

            await ReplyAsync(embed: builder.Build());
        }

        [Command("legendaries")]
        public async Task LegendariesAsync()
        {
            Dal.DoConnection();
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"{Dal.GetEveryPossess()} have been found in total.")
                .WithColor(Color.DarkGreen);
            await ReplyAsync(embed: builder.Build());

            Dal.CloseConnection();

        }

        // Top lucky players ?
        [Command("luckranks")]
        [Alias("lr")]
        public async Task LuckRanksAsync()
        {
            Dal.DoConnection();

            List<ServerUser> everyUsers = Dal.GetEveryUser(Context.Guild.Id.ToString());

            EmbedBuilder builder = new EmbedBuilder();
            string aString = "";

            List<ServerUser> sorted = Helper.GetLuckRanking(everyUsers);

            for(int i = 0; i < sorted.Count; i++)
            {
                if(sorted[i].DiscordID == Context.User.Id.ToString())
                    aString += $"**#{i + 1} : {sorted[i].DiscordName} with {sorted[i].GetLuck()}**" + Environment.NewLine;
                else
                    aString += $"#{i + 1} : {sorted[i].DiscordName} with {sorted[i].GetLuck()}" + Environment.NewLine;
            }

            builder.AddField($"Top 10 luckiest players", aString);
            builder.WithColor(Color.DarkGreen);
            
            Dal.CloseConnection();

            await ReplyAsync(embed: builder.Build());
        }


        [Command("search rc")]
        public async Task SearchRCAsync()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"{Context.User.Username} just found nothing in the Research Center...")
                .WithColor(Color.LightGrey);
            await ReplyAsync("", false, builder.Build());

        }

        [Command("search research center")]
        public async Task SearchResearchAsync()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"{Context.User.Username} just found nothing in the Research Center...")
                .WithColor(Color.LightGrey);
            await ReplyAsync("", false, builder.Build());

        }

        [Command("search legendary")]
        public async Task SearchLegAsync()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($":clap: :yellow_heart: :clap: {Context.User.Username} just found a super legendary item !!!!! Nice joke man :clap: :yellow_heart: :clap:")
                            .WithColor(Color.Gold);
            await ReplyAsync("", false, builder.Build());

        }







    }
}
