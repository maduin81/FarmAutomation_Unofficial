using Microsoft.Xna.Framework;
using StardewValley;

namespace FarmAutomation.Common
{
    public class LocationHelper
    {
        public static string GetName(GameLocation location)
        {
            return location.uniqueName ?? location.Name;
        }

        public static bool IsTileOnMap(GameLocation location, Vector2 position)
        {
            return location.Objects.ContainsKey(position) || location.terrainFeatures.ContainsKey(position);
        }
    }
}
