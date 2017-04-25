using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FarmAutomation.Common;
using FarmAutomation.ItemCollector.Models;
using FarmAutomation.ItemCollector.Processors;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FarmAutomation.ItemCollector
{
    public class ItemCollectorMod : Mod
    {
        private MachinesProcessor _machinesProcessor;
        private BuildingProcessor _buildingProcessor;
        private ItemCollectorConfiguration _config;
        private List<MachineBuildingConfig> _machineBuildingConfigs;

        private bool _gameLoaded;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadJsonFile<ItemCollectorConfiguration>("ItemCollectorConfiguration.json");

            GameEvents.GameLoaded += GameEvents_GameLoaded;
            TimeEvents.DayOfMonthChanged += TimeEvents_DayOfMonthChanged;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
            PlayerEvents.InventoryChanged += PlayerEvents_InventoryChanged;
#if DEBUG
            // allow keypresses to initiate events for easier debugging.
            ControlEvents.KeyPressed += this.ControlEvents_KeyPressed;
#endif
        }

        private void GameEvents_GameLoaded(object sender, EventArgs e)
        {
            _gameLoaded = true;

            _InitializeMod();

            try
            {
                var pathOnDisk = Helper.DirectoryPath;

                File.WriteAllLines(Path.Combine(pathOnDisk, "Objects.txt"), Game1.objectInformation.Select(t => t.Key.ToString() + " - " + t.Value).ToArray());
                File.WriteAllLines(Path.Combine(pathOnDisk, "CraftingItems.txt"), Game1.bigCraftablesInformation.Select(t => t.Key.ToString() + " - " + t.Value).ToArray());
            }
            catch
            {
                Monitor.Log("Unable to write item files.  Skipping.");
            }
        }

        private void TimeEvents_DayOfMonthChanged(object sender, EventArgsIntChanged e)
        {
            if (_config.EnableMod)
            {
                Monitor.Log("It's a new day. Resetting the Item Collector mod");
                _machinesProcessor.ValidateGameLocations();
                _buildingProcessor.DailyReset();
                _machinesProcessor.DailyReset();
            }
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            if (_gameLoaded && _config.EnableMod)
            {
                try
                {
                    _buildingProcessor.ProcessAnimalBuildings();
                    _buildingProcessor.ProcessMachineBuildings(_machineBuildingConfigs);
                    _machinesProcessor.ProcessMachines();
                }
                catch (Exception ex)
                {
                    Monitor.Log($"an error occured with the automation mod: {ex}", LogLevel.Error);
                    _machinesProcessor.DailyReset();
                }
            }
        }

        private void PlayerEvents_InventoryChanged(object sender, EventArgsInventoryChanged e)
        {
            if (_gameLoaded && ItemFinder.HaveConnectorsInInventoryChanged(e))
            {
                try
                {
                    _buildingProcessor.DailyReset();
                    _machinesProcessor.InvalidateCacheForLocation(Game1.player.currentLocation);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"an error occured with the automation mod: {ex}", LogLevel.Error);
                    _machinesProcessor.DailyReset();
                }
            }
        }

#if DEBUG
        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (_gameLoaded && e.KeyPressed == Keys.K)
            {
                _buildingProcessor.ProcessAnimalBuildings();
                _machinesProcessor.ProcessMachines();
            }

            if (_gameLoaded && e.KeyPressed == Keys.P)
            {
                _buildingProcessor.DailyReset();
                _machinesProcessor.DailyReset();
            }
        }
#endif

        private static List<AcceptableObject> _GetSeedMakerMaterials()
        {
            return (from v in Game1.temporaryContent.Load<Dictionary<int, string>>(@"Data\Crops").Values
                    select v.Split('/') into s
                    select new AcceptableObject(Convert.ToInt32(s[3]))).ToList();
        }

        private void _InitializeMod()
        {
            Monitor.Log($"Initalizing {nameof(ItemCollectorMod)}", LogLevel.Info);

            _config.MachineConfigs.ForEach(t =>
            {
                if (t.AcceptableObjects == null)
                {
                    t.AcceptableObjects = new List<AcceptableObject>();
                }

                if (t.AcceptableCategories == null)
                {
                    t.AcceptableCategories = new List<AcceptableCategory>();
                }
            });

            _machineBuildingConfigs = _config.MachineBuildingConfigs;

            var machineConfigs = _config.MachineConfigs;
            var config = machineConfigs.FirstOrDefault(t => t.Name == "Seed Maker");

            if (config != null)
            {
                config.AcceptableObjects = _GetSeedMakerMaterials();
            }

            ItemFinder.ConnectorItems = machineConfigs.Where(t => t.AllowConnection).ToList().Select(t => t.Name).ToList();
            ItemFinder.ConnectorFloorings = _config.FlooringsToConsiderConnectors;

            var locationsToSearch = _config.LocationsToSearch.Split(',').Select(v => v.Trim()).ToList();

            _machinesProcessor = new MachinesProcessor(machineConfigs, locationsToSearch, _config.AddBuildingsToLocations, _config.AllowDiagonalConnectionsForAllItems, Monitor)
            {
                MuteWhileCollectingFromMachines = Math.Max(0, Math.Min(5000, _config.MuteWhileCollectingFromMachines))
            };

            _buildingProcessor = new BuildingProcessor(_config.PetAnimals, _config.AdditionalFriendshipFromCollecting, _config.MuteAnimalsWhenCollecting, _config.BuildingsToIgnore, Monitor);
        }
    }
}
