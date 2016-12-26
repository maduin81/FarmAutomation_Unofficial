using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarmAutomation.Common;
using FarmAutomation.ItemCollector.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace FarmAutomation.ItemCollector.Processors
{
    public class MachinesProcessor
    {
        private static IMonitor _monitor;

        private readonly List<string> _gameLocationsToSearch;

        private readonly bool _allowDiagonalConnectionsForAllItems;

        private readonly MaterialHelper _materialHelper;

        private readonly List<MachineConfig> _machineConfigs;        

        private Dictionary<string, Dictionary<Vector2, Chest>> _connectedChestsCache = new Dictionary<string, Dictionary<Vector2, Chest>>();

        public MachinesProcessor(List<MachineConfig> machineConfigs, List<string> gameLocationsToSearch, bool addBuildingsToLocations, bool allowDiagonalConnectionsForAllItems, IMonitor monitor)
        {
            _monitor = monitor;
            AddBuildingsToLocations = addBuildingsToLocations;
            _machineConfigs = (from x in machineConfigs
                               where x.CollectFrom || x.InsertItems
                               select x).ToList();
            _gameLocationsToSearch = gameLocationsToSearch;
            _allowDiagonalConnectionsForAllItems = allowDiagonalConnectionsForAllItems;
            _gameLocationsToSearch.ForEach(gl => _connectedChestsCache.Add(gl, new Dictionary<Vector2, Chest>()));
            _materialHelper = new MaterialHelper();
            DailyReset();
        }

        public bool AddBuildingsToLocations { get; set; }

        public int MuteWhileCollectingFromMachines { get; set; }

        public IEnumerable<GameLocation> GetLocations()
        {
            var gameLocations = new List<GameLocation>();
            lock (_gameLocationsToSearch)
            {
                foreach (var locationName in _gameLocationsToSearch)
                {
                    var location = Game1.getLocationFromName(locationName);
                    if (location != null)
                    {
                        gameLocations.Add(location);

                        var farm = location as Farm;
                        if (farm != null && AddBuildingsToLocations)
                        {
                            gameLocations.AddRange(farm.buildings.Where(building => building?.indoors != null).Select(building => building.indoors));
                        }
                    }
                }
            }

            return gameLocations;
        }

        public void ValidateGameLocations()
        {
            var locations = string.Join(", ", Game1.locations.Select(l => l.Name));
            _monitor.Log($"Loading locations. These are all the currently known locations in the game:\r\n{locations}", LogLevel.Info);

            lock (_gameLocationsToSearch)
            {
                foreach (var locationName in _gameLocationsToSearch.ToList())
                {
                    var location = Game1.getLocationFromName(locationName);
                    if (location == null)
                    {
                        _monitor.Log($"Could not find a location with the name of '{locationName}'", LogLevel.Error);
                        _gameLocationsToSearch.Remove(locationName);
                    }
                }
            }
        }

        public void ProcessMachines()
        {
            if (_connectedChestsCache == null)
            {
                _connectedChestsCache = new Dictionary<string, Dictionary<Vector2, Chest>>();
                Parallel.ForEach(GetLocations(), BuildCacheForLocation);
            }

            if (MuteWhileCollectingFromMachines > 0)
            {
                SoundHelper.MuteTemporary(MuteWhileCollectingFromMachines);
            }

            foreach (var gameLocation in GetLocations())
            {
                MachineHelper.Who.currentLocation = gameLocation;
                MachineHelper.Monitor = _monitor;
                lock (_connectedChestsCache)
                {
                    if (!_connectedChestsCache.ContainsKey(LocationHelper.GetName(gameLocation)))
                    {
                        // cache got invalidated
                        BuildCacheForLocation(gameLocation);
                    }
                }

                foreach (var valuePair in _connectedChestsCache[LocationHelper.GetName(gameLocation)])
                {
                    var location = valuePair.Key;
                    var connectedChest = valuePair.Value;
                    if (connectedChest != null && gameLocation.objects.ContainsKey(location) &&
                        _machineConfigs.Any(x => x.Name == gameLocation.objects[location].Name))
                    {
                        MachineHelper.ProcessMachine(gameLocation.objects[location], connectedChest, _materialHelper, (from x in _machineConfigs where x.Name == gameLocation.objects[location].Name select x).FirstOrDefault());
                    }
                }
            }
        }

        public void DailyReset()
        {
            if (Game1.hasLoadedGame)
            {
                MachineHelper.DailyReset();
            }

            _connectedChestsCache = null;
        }

        public void InvalidateCacheForLocation(GameLocation location)
        {
            if (_connectedChestsCache != null && location != null && _connectedChestsCache.ContainsKey(LocationHelper.GetName(location)))
            {
                _connectedChestsCache.Remove(LocationHelper.GetName(location));
            }
        }

        private void BuildCacheForLocation(GameLocation gameLocation)
        {
            if (gameLocation != null)
            {
                var cacheToAdd = new Dictionary<Vector2, Chest>();
                _monitor.Log($"Starting search for connected locations at {LocationHelper.GetName(gameLocation)}");
                var items = ItemFinder.FindObjectsWithName(gameLocation, (from x in _machineConfigs select x.Name).ToList());
                foreach (var valuePair in items)
                {
                    var location = valuePair.Key;
                    if (cacheToAdd.ContainsKey(location))
                    {
                        // already found in another search
                        continue;
                    }

                    var processedLocations = new List<ConnectedTile>
                    {
                        new ConnectedTile { Location = location, Object = valuePair.Value }
                    };

                    ItemFinder.FindConnectedLocations(gameLocation, location, processedLocations, _allowDiagonalConnectionsForAllItems);
                    var chest = processedLocations.FirstOrDefault(c => c.Chest != null)?.Chest;
                    foreach (var connectedLocation in processedLocations)
                    {
                        cacheToAdd.Add(connectedLocation.Location, chest);
                    }
                }

                lock (_connectedChestsCache)
                {
                    if (_connectedChestsCache.ContainsKey(LocationHelper.GetName(gameLocation)))
                    {
                        // already ran?
                        _connectedChestsCache.Remove(LocationHelper.GetName(gameLocation));
                    }

                    _connectedChestsCache.Add(LocationHelper.GetName(gameLocation), new Dictionary<Vector2, Chest>());
                    foreach (var cache in cacheToAdd)
                    {
                        _connectedChestsCache[LocationHelper.GetName(gameLocation)].Add(cache.Key, cache.Value);
                    }
                }

                _monitor.Log($"Searched your {LocationHelper.GetName(gameLocation)} for machines to collect from and found a total of {_connectedChestsCache[LocationHelper.GetName(gameLocation)].Count} locations to look for");
            }
        }        
    }
}
