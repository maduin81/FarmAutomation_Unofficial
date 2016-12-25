using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace FarmAutomation.ItemCollector.Models
{
      internal class MachineBuilding
      {
            public MachineBuildingConfig BuildingConfig { get; set; }

            public Mill Mill { get; set; }

            public JunimoHut Hut { get; set; }

            public string BuildingType { get; set; }

            public Chest Input { get; set; }

            public Chest Output { get; set; }

            public Vector2 InputLocation { get; set; }

            public Vector2 OutputLocation { get; set; }
      }
}
