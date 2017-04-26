using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace FarmAutomation.Common
{
    /// <summary>
    /// helper class for actions where a reference to a farmer is necessary
    /// </summary>
    public class GhostFarmer : SFarmer
    {
        private GhostFarmer() : base(new FarmerSprite(null), Vector2.Zero, 1, "GhostFarmer", new List<Item>(), true)
        {
            ClearInventory();
            uniqueMultiplayerID = Game1.player.uniqueMultiplayerID;
            professions = Game1.player.professions;
            FarmerSprite.setOwner(this);
            maxItems = 24;
        }

        /// <summary>
        /// need to override the constructor as for some reason the base sets the sprites on the main player which leads to a crash.
        /// </summary>
        /// <returns>Returns GhostFarmer</returns>
        public static GhostFarmer CreateFarmer()
        {
            var prevSprite = Game1.player.sprite;
            var who = new GhostFarmer();
            Game1.player.sprite = prevSprite;
            return who;
        }

        public void ClearInventory()
        {
            items = new List<Item>(new Item[maxItems]);
        }
    }
}