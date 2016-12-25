using System.Collections.Generic;
using FarmAutomation.Common;
using FarmAutomation.ItemCollector.Models;

namespace FarmAutomation.ItemCollector
{
    public class ItemCollectorConfiguration : ConfigurationBase
    {
        public ItemCollectorConfiguration()
        {
            FlooringsToConsiderConnectors = new List<int>();
        }

        public bool PetAnimals { get; set; }

        public int AdditionalFriendshipFromCollecting { get; set; }

        public bool MuteAnimalsWhenCollecting { get; set; }

        public string MachinesToCollectFrom { get; set; }

        public string ItemsToConsiderConnectors { get; set; }

        public bool AllowDiagonalConnectionsForAllItems { get; set; }

        public List<int> FlooringsToConsiderConnectors { get; set; }

        public string LocationsToSearch { get; set; }

        public bool AddBuildingsToLocations { get; set; }

        public int MuteWhileCollectingFromMachines { get; set; }

        public List<MachineConfig> MachineConfigs { get; set; }

        public List<MachineBuildingConfig> MachineBuildingConfigs { get; set; }

        public override void InitializeDefaults()
        {
            PetAnimals = true;
            AdditionalFriendshipFromCollecting = 5;
            EnableMod = true;
            LocationsToSearch = "Farm, Greenhouse, FarmHouse, FarmCave, Beach, Cellar";
            FlooringsToConsiderConnectors = new List<int> { 6 };
            AddBuildingsToLocations = true;
            MachineConfigs = new List<MachineConfig>
            {
                new MachineConfig("Furnace", true)
                {
                    CoalRequired = 1,
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject(380, 5),
                        new AcceptableObject(384, 5),
                        new AcceptableObject(378, 5),
                        new AcceptableObject(386, 5),
                        new AcceptableObject(80)
                    }
                },
                new MachineConfig("Preserves Jar", true)
                {
                    AcceptableCategories = new List<AcceptableCategory>
                    {
                        new AcceptableCategory(-79),
                        new AcceptableCategory(-75)
                    }
                },
                new MachineConfig("Keg", true)
                {
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject(433, 5),
                        new AcceptableObject(340),
                        new AcceptableObject(262),
                        new AcceptableObject(304)
                    },
                    AcceptableCategories = new List<AcceptableCategory>
                    {
                        new AcceptableCategory(-79),
                        new AcceptableCategory(-75)
                    }
                },
                new MachineConfig("Loom", true)
                {
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject(440)
                    }
                },
                new MachineConfig("Cheese Press", true)
                {
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject(184),
                        new AcceptableObject(186),
                        new AcceptableObject(436),
                        new AcceptableObject(438)
                    }
                },
                new MachineConfig("Mayonnaise Machine", true)
                {
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject(107)
                    },
                    AcceptableCategories = new List<AcceptableCategory>
                    {
                        new AcceptableCategory(-5)
                    }
                },
                new MachineConfig("Oil Maker", true)
                {
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject(270),
                        new AcceptableObject(420),
                        new AcceptableObject(430),
                        new AcceptableObject(431)
                    }
                },
                new MachineConfig("Recycling Machine", true)
                {
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject(168),
                        new AcceptableObject(169),
                        new AcceptableObject(170),
                        new AcceptableObject(171),
                        new AcceptableObject(172)
                    }
                },
                new MachineConfig("Charcoal Kiln", true)
                {
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject(388, 10)
                    }
                },
                new MachineConfig("Slime Egg-Press", true)
                {
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject(766, 100)
                    }
                },
                new MachineConfig("Cask", true, true, true, 2)
                {
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject(459),
                        new AcceptableObject(424),
                        new AcceptableObject(426),
                        new AcceptableObject(459),
                        new AcceptableObject(303),
                        new AcceptableObject(346),
                        new AcceptableObject(348)
                    }
                },
                new MachineConfig("Crab Pot", true)
                {
                    AcceptableCategories = new List<AcceptableCategory>
                    {
                        new AcceptableCategory(-21)
                    }
                },
                new MachineConfig("Crystalarium"),
                new MachineConfig("Worm Bin"),
                new MachineConfig("Bee House"),
                new MachineConfig("Strange Capsule"),
                new MachineConfig("Tapper"),
                new MachineConfig("Statue Of Endless Fortune"),
                new MachineConfig("Statue Of Perfection"),
                new MachineConfig("Mushroom Box"),
                new MachineConfig("Lightning Rod"),
                new MachineConfig("Chest", false, false),
                new MachineConfig("Seed Maker", true)
            };

            MachineBuildingConfigs = new List<MachineBuildingConfig>
            {
                new MachineBuildingConfig
                {
                    Name = "Mill",
                    InputXOffset = 1,
                    InputYOffset = 1,
                    OutputXOffset = 3,
                    OutputYOffset = 1,
                    HasInput = true,
                    HasOutput = true,
                    AcceptableObjects = new List<AcceptableObject>
                    {
                        new AcceptableObject("Beet"),
                        new AcceptableObject("Wheat")
                    }
                },
                new MachineBuildingConfig
                {
                    Name = "Junimo Hut",
                    OutputXOffset = 0,
                    OutputYOffset = 1,
                    HasOutput = true
                }
            };
        }
    }
}
