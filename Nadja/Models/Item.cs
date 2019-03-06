﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Nadja.Models
{
    public class Item
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Slangs { get; set; }

        public Craft Craft { get; set; }

        public List<Found> Founds { get; set; }
        
        public bool IsNamed(string way)
        {
            if (Name.ToLower() == way.ToLower())
                return true;

            foreach (string slang in Slangs)
                if (slang.ToLower() == way.ToLower())
                    return true;

            return false;
        }

        public string DisplayLocationItem()
        {
            string result = "";
            List<Found> tempList = new List<Found>(Founds.ToArray());
            while(tempList.Count > 0)
            {
                string tempString = "";
                double maxPercentage = Math.Round(100 * (double)tempList[0].Amount / tempList[0].Location.GetTotalItemsInArea(), 2);
                Found tempFind = null;
                foreach(Found find in tempList)
                {
                    
                    double percentage = Math.Round(100 * (double)find.Amount / find.Location.GetTotalItemsInArea(), 2);
                    if (percentage >= maxPercentage)
                    {
                        tempFind = find;
                        tempString = $"{find.Location.Name} ({find.Amount} / {find.Location.GetTotalItemsInArea()}) = {percentage}%\n";
                        maxPercentage = percentage;
                    }
                    if (find.Location.Name == "Random")
                    {
                        tempFind = find;
                        tempString = $"Random\n";
                        break;
                    }
                }

                tempList.Remove(tempFind);
                result += tempString;
            }
            
            
            switch (Name)
            {
                case ("Bird Egg"):
                    result += "Egg Crow\n";
                    break;
                case ("Bird Meat"):
                    result += "Meat Crow\n";
                    break;
                case ("Flower"):
                    result += "Bear \n";
                    break;
                case ("Garlic"):
                    result += "Bear \n";
                    break;
                case ("Burdock"):
                    result += "Gorilla \n";
                    break;
                case ("Fertilizer"):
                    result += "Gorilla \n";
                    break;
                case ("First Aid Box"):
                    result += "Dr. Wickeline \n";
                    break;
                case ("Holy Blood"):
                    result += "Dr. Wickeline \n";
                    break;
                case ("Cigarettes"):
                    result += "Dr. Meiji \n";
                    break;
                case ("Bull Intestine"):
                    result += "Dr. Meiji \n";
                    break;
                case ("Heartbeat Sensor"):
                    result += "Dr. Meiji \n";
                    break;
                case ("Feather"):
                    result += "Water crow \n" +
                        "Bread crow \n";
                    break;
                case ("Leather"):
                    result += "Dog \n";
                    break;
                case ("Fountain Pen"):
                    result += "Pen dog \n";
                    break;
                case ("Lighter"):
                    result += "Lighter dog \n";
                    break;
            }

            return result;
        }



        public void DisplayOnlyLocationItem(EmbedBuilder builder)
        {
            string aString = DisplayLocationItem();
            if (aString == "")
                aString = "There is no way to find this item.";

            builder.AddField($"To find {Name} :", aString, true);
        }

        public void DisplayItem(EmbedBuilder builder, bool first)
        {
            if (Craft != null)
            {
                Craft.DisplayCraft(builder, first);

                // If items for crafting the askingItem exists, display it too using this function

            }
        }
    }
}
