using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.IO;
using System.Linq;
using MailFrameworkMod;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System.Collections.Generic;

namespace SDVMod
{
    public class ModEntry : Mod, IAssetLoader
    {
        internal static ModEntry Instance { get; private set; }
        //internal JsonAssets jsonAssets { get; private set; }
        private IModHelper _helper;
        class ModData
        {
            public long TMELD { get; set; } = Game1.player.totalMoneyEarned;
        }
        public class Config
        {
            public bool showcrops { get; set; } = false;
            public bool sendrecipes { get; set; } = false;
        }
        public Config config;
        public class RandomColorWidgetState
        {
            public Color color;
        }
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.DayStarted += DayStart;
            helper.Events.GameLoop.DayEnding += DayEnd;
            helper.Events.GameLoop.GameLaunched += GameLaunch;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Player.Warped += OnWarped;
            //helper.Events.GameLoop.GameLaunched += onGameLaunched;
        }
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Name.Equals("LostWoods"))
            {
                Game1.changeMusicTrack("woodsTheme");
            }
        }
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs args)
        {
            string LWK = this.Helper.Content.GetActualAssetKey("assets/LostWoods.tbin", ContentSource.ModFolder);
            GameLocation LW = new GameLocation(LWK, "LostWoods") { IsOutdoors = false, IsFarm = false };
            Game1.locations.Add(LW);

            string LWHK = this.Helper.Content.GetActualAssetKey("assets/LWH.tbin", ContentSource.ModFolder);
            GameLocation LWH = new GameLocation(LWHK, "LostWoodsHouse1") { IsOutdoors = false, IsFarm = false };
            Game1.locations.Add(LWH);
        }
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"Maps/Beach");
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            //if (asset.AssetNameEquals(@"Maps/Beach"))
            return this.Helper.Content.Load<T>(@"assets/Beach.tbin");
        }
        public string GetCurrentRecipeId()
        {
            int whichWeek = (((int)(Game1.stats.DaysPlayed % 224u / 7u)) % 32) + 1;
            Dictionary<string, string> cookingRecipeChannel = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");
            return cookingRecipeChannel[whichWeek.ToString()].Split('/')[0];
        }
        private void GameLaunch(object sender, GameLaunchedEventArgs e)
        {
            config = Helper.ReadConfig<Config>();
            var api = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            api.RegisterModConfig(ModManifest, () => config = new Config(), () => Helper.WriteConfig(config));
            api.RegisterSimpleOption(ModManifest, "Show Total Crops at daystart", "Checking this will show your total crops on your farm every morning!", () => config.showcrops, (bool val) => config.showcrops = val);
            api.RegisterSimpleOption(ModManifest, "Send QOFTS Recipes", "Checking this will send every queen of the sauce recipe you dont have to your mail!", () => config.sendrecipes, (bool val) => config.sendrecipes = val);
        }

        // dawprivate void onGameLaunched (object _sender, GameLaunchedEventArgs _e)
        //{
        // Load the Json Assets content pack.
        //jsonAssets = Helper.ModRegistry.GetApi<JsonAssets.IApi>("spacechase0.JsonAssets");
        //if (jsonAssets == null)
        //{
        //Monitor.LogOnce ("Could not connect to Json Assets. It may not be installed or working properly",
        //LogLevel.Error);
        //return;
        //}
        //jsonAssets.LoadAssets (Path.Combine (Helper.DirectoryPath,
        //"assets", "JA"));
        // Set up Generic Mod Config Menu, if it is available.
        //}


        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            uint totalEarnings = Game1.player.totalMoneyEarned;
            //if (totalEarnings > 500000)
                //if (Game1.currentSeason.Equals("spring"))
                    //MailDao.SaveLetter(
                        //new Letter(
                            //"Raik1",
                            //"Hey, i send you this recipe:^Season Salad (Spring) have fun^^-Raik",
                            //"Seasonal Salad (Spring)",
                            //(l) => !Game1.player.cookingRecipes.ContainsKey(l.Recipe)
                        //)
                    //);
            //if (Game1.currentSeason.Equals("summer"))
                //MailDao.SaveLetter(
                    //new Letter(
                        //"Raik2",
                        //"Hey, i send you this recipe:^Season Salad (Summer) have fun^^-Raik",
                        //"Seasonal Salad (Summer)",
                        //(l) => !Game1.player.cookingRecipes.ContainsKey(l.Recipe)
            //)
        //);
        }
        public void DayEnd(object sender, DayEndingEventArgs d)
        {

        }
        private void DayStart(object sender, DayStartedEventArgs d)
        {
            int Crops = Game1.getFarm().terrainFeatures.Values.Count((tf) => tf is StardewValley.TerrainFeatures.HoeDirt hd && hd.crop != null);
            if (config.showcrops.Equals(true))
            {
                Game1.addHUDMessage(new HUDMessage($"Crops on Farm: {Crops}", 2));
                if (Game1.dayOfMonth.Equals(28))
                {
                }
            }
            if (config.sendrecipes.Equals(true))
            {
                // string[] weeklyRecipe = Helper.Reflection.GetMethod(new TV(), "getWeeklyRecipe").Invoke<string[]>();
                // string Recipe = weeklyRecipe[1].Replace(".", ""); // string.Join("", weeklyRecipe).Substring(0, string.Join("", weeklyRecipe).IndexOf("!") + 1).Replace("!", "");
                // if (!Recipe.StartsWith("You already know how to cook "))
                // this.Monitor.Log($"Recipe: {Recipe}", LogLevel.Debug);
                string Recipe = GetCurrentRecipeId();
                this.Monitor.Log($"Recipe: {Recipe}",LogLevel.Debug);
                if (Recipe != "")
                   MailDao.SaveLetter(
                       new Letter(
                           $"{Recipe}",
                           $"Weekly Recipe: {Recipe}^Instructions to cook included^^-Queen of the sauce leader",
                           $"{Recipe}",
                           (l) => !Game1.player.cookingRecipes.ContainsKey(l.Recipe)
               )
           );
            }
        }
    }
}