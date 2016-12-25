using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FarmAutomation.Common;
using FarmAutomation.ItemCollector.Models;
using FarmAutomation.ItemCollector.Processors;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FarmAutomation.ItemCollector
{
    public class ItemCollectorMod : Mod
    {        
        private readonly MachinesProcessor _machinesProcessor;
        private readonly BuildingProcessor _buildingProcessor;
        private readonly ItemCollectorConfiguration _config;
        private readonly List<MachineBuildingConfig> _machineBuildingConfigs;

        private bool _gameLoaded;

        public ItemCollectorMod()
        {
            Log.Info($"Initalizing {nameof(ItemCollectorMod)}");
            _config = ConfigurationBase.LoadConfiguration<ItemCollectorConfiguration>();
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
                config.AcceptableObjects = GetSeedMakerMaterials();
            }

            ItemFinder.ConnectorItems = new List<string>(_config.ItemsToConsiderConnectors.Split(',').Select(v => v.Trim()));
            ItemFinder.ConnectorFloorings = _config.FlooringsToConsiderConnectors;
            
            var locationsToSearch = _config.LocationsToSearch.Split(',').Select(v => v.Trim()).ToList();
            _machinesProcessor = new MachinesProcessor(machineConfigs, locationsToSearch, _config.AddBuildingsToLocations, _config.AllowDiagonalConnectionsForAllItems)
            {
                MuteWhileCollectingFromMachines = Math.Max(0, Math.Min(5000, _config.MuteWhileCollectingFromMachines))
            };

            _buildingProcessor = new BuildingProcessor(_config.PetAnimals, _config.AdditionalFriendshipFromCollecting, _config.MuteAnimalsWhenCollecting);
        }

        public override void Entry(params object[] objects)
        {
            base.Entry(objects);
            GameEvents.GameLoaded += (s, e) => { _gameLoaded = true; };

            try
            {
                var pathOnDisk = PathOnDisk;
                File.WriteAllLines(Path.Combine(pathOnDisk, "Objects.txt"), Game1.objectInformation.Select(t => t.Key.ToString() + " - " + t.Value).ToArray());
                File.WriteAllLines(Path.Combine(pathOnDisk, "CraftingItems.txt"), Game1.bigCraftablesInformation.Select(t => t.Key.ToString() + " - " + t.Value).ToArray());
            }
            catch
            {
                Log.Debug("Unable to write item files.  Skipping.");
            }

            TimeEvents.DayOfMonthChanged += (s, e) =>
            {
                if (_config.EnableMod)
                {
                    Log.Debug("It's a new day. Resetting the Item Collector mod");
                    _machinesProcessor.ValidateGameLocations();
                    _buildingProcessor.DailyReset();
                    _machinesProcessor.DailyReset();
                }
            };
            TimeEvents.TimeOfDayChanged += (s, e) =>
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
                        Log.Error($"an error occured with the automation mod: {ex}");
                        _machinesProcessor.DailyReset();
                    }
                }
            };
            PlayerEvents.InventoryChanged += (s, c) =>
            {
                if (_gameLoaded && ItemFinder.HaveConnectorsInInventoryChanged(c))
                {
                    try
                    {
                        _buildingProcessor.DailyReset();
                        _machinesProcessor.InvalidateCacheForLocation(Game1.player.currentLocation);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"an error occured with the automation mod: {ex}");
                        _machinesProcessor.DailyReset();
                    }
                }
            };
#if DEBUG
            // allow keypresses to initiate events for easier debugging.
            ControlEvents.KeyPressed += (s, c) =>
            {
                if (_gameLoaded && c.KeyPressed == Keys.K)
                {
                    _buildingProcessor.ProcessAnimalBuildings();
                    _machinesProcessor.ProcessMachines();
                }

                if (_gameLoaded && c.KeyPressed == Keys.P)
                {
                    _buildingProcessor.DailyReset();
                    _machinesProcessor.DailyReset();
                }
            };
#endif
        }

        private static List<AcceptableObject> GetSeedMakerMaterials()
        {
            if (Game1.temporaryContent == null)
            {
                Game1.temporaryContent = new LocalizedContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
            }

            return (from v in Game1.temporaryContent.Load<Dictionary<int, string>>(@"Data\Crops").Values
                    select v.Split('/') into s
                    select new AcceptableObject(Convert.ToInt32(s[3]))).ToList();
        }
    }
}
