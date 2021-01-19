namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.FeaturesSlideshow.Data
{
    using System.Collections.Generic;

    public static class FeaturesSlideshowEntries
    {
        public const string EntryCharacters_Description =
            "Customize the appearance of your character and use a variety of different equipment and tools to differentiate yourself.";

        public const string EntryCharacters_Title = "Character customization";

        public const string EntryCreatures_Description =
            "Explore a diverse world with a multitude of different biomes and dozens of fantastic creatures.";

        public const string EntryCreatures_Title = "Fantastic creatures";

        public const string EntryEconomy_Description =
            "Mint coins and build fully automated trading stations to facilitate trade with other survivors.";

        public const string EntryEconomy_Title = "Economy";

        public const string EntryElectricity_Description =
            "Design your base with an electricity grid in mind. You can build a number of generators, storage structures and electricity consumers.";

        public const string EntryElectricity_Title = "Electricity";

        public const string EntryEvents_Description =
            "Participate in the many events happening in the world of CryoFall regularly. Be it exploring the crash site of a meteorite or gathering a group of survivors to defeat a dangerous boss.";

        public const string EntryEvents_Title = "World events";

        public const string EntryFarming_Description =
            "Grow your own crops, using fertilizers and farming tools. Then use the fruits of your labor to cook different food, which can provide active benefits to your character.";

        public const string EntryFarming_Title = "Farming & cooking";

        public const string EntryFishing_Description =
            "Use different types of bait and catch a variety of freshwater and saltwater fish. Try to beat your records and compete with other players.";

        public const string EntryFishing_Title = "Fishing";

        public const string EntryImplants_Description =
            "Install a variety of different bionic and cybernetic implants into your character to receive new abilities or boost your stats.";

        public const string EntryImplants_Title = "Cybernetic implants";

        public const string EntryIndustry_Description =
            "Exploit natural resources available in the world of CryoFall. Pump out and refine petroleum oil or extract lithium from geothermal springs to make batteries and electronics.";

        public const string EntryIndustry_Title = "Industry";

        public const string EntryLastWord_Description =
            @"And finally, please keep in mind that CryoFall is still a work in progress, and a lot of improvements and changes are planned. Nevertheless, it is a very solid game already and we hope you can thoroughly enjoy it![br][br]
              We are also always available on Discord and our forums—come say hi! :)[br][br]
              If you have any difficulties or would like to report a bug or exploit, we are willing to listen and help!";

        public const string EntryLastWord_Title = "Last word";

        public const string EntryMedicine_Description =
            "Complex character simulation not only with the usual hunger and thirst mechanics, but also a multitude of status effects and afflictions, which can be addressed using a variety of medical items.";

        public const string EntryMedicine_Title = "Character simulation & medicine";

        public const string EntrySkills_Description =
            "Discover, unlock and improve a variety of skills for your character, helping you increase efficiency and gain new abilities.";

        public const string EntrySkills_Title = "Skills";

        public const string EntryTechnology_Description =
            "Gain learning points and use them to learn new technologies, recipes and buildings, customizing the game to your liking by focusing on the areas that interest you the most.";

        public const string EntryTechnology_Title = "Technology";

        public const string EntryVehicles_Description =
            "Build and upgrade a variety of futuristic vehicles to explore the world faster, or use them in combat.";

        public const string EntryVehicles_Title = "Vehicles";

        public const string EntryWelcome_Description =
            "We would like to take this opportunity to introduce you to some of the features you can expect in the game! CryoFall is still in active development but all of the following is already available.";

        public const string EntryWelcome_Title = "Welcome to CryoFall!";

        public static readonly IReadOnlyList<Entry> Entries = new[]
        {
            new Entry(EntryWelcome_Title,
                      EntryWelcome_Description,
                      "Features/Welcome.png"),
            new Entry(EntryFarming_Title,
                      EntryFarming_Description,
                      "Features/Farming.png"),
            new Entry(EntryMedicine_Title,
                      EntryMedicine_Description,
                      "Features/Medicine.png"),
            new Entry(EntryIndustry_Title,
                      EntryIndustry_Description,
                      "Features/Industry.png"),
            new Entry(EntryImplants_Title,
                      EntryImplants_Description,
                      "Features/Implants.png"),
            new Entry(EntryTechnology_Title,
                      EntryTechnology_Description,
                      "Features/Technology.png"),
            new Entry(EntrySkills_Title,
                      EntrySkills_Description,
                      "Features/Skills.png"),
            new Entry(EntryCharacters_Title,
                      EntryCharacters_Description,
                      "Features/Characters.png"),
            new Entry(EntryEconomy_Title,
                      EntryEconomy_Description,
                      "Features/Economy.png"),
            new Entry(EntryFishing_Title,
                      EntryFishing_Description,
                      "Features/Fishing.png"),
            new Entry(EntryElectricity_Title,
                      EntryElectricity_Description,
                      "Features/Electricity.png"),
            new Entry(EntryVehicles_Title,
                      EntryVehicles_Description,
                      "Features/Vehicles.png"),
            new Entry(EntryEvents_Title,
                      EntryEvents_Description,
                      "Features/Events.png"),
            new Entry(EntryCreatures_Title,
                      EntryCreatures_Description,
                      "Features/Creatures.png"),
            new Entry(EntryLastWord_Title,
                      EntryLastWord_Description,
                      "Features/LastWord.jpg")
        };

        public class Entry
        {
            public Entry(string title, string description, string textureImagePath)
            {
                this.Title = title;
                this.Description = description;
                this.TextureImagePath = textureImagePath;
            }

            public string Description { get; }

            public string TextureImagePath { get; }

            public string Title { get; }
        }
    }
}