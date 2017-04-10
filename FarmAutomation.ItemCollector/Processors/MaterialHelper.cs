using System.Collections.Generic;
using System.Linq;
using FarmAutomation.ItemCollector.Models;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace FarmAutomation.ItemCollector.Processors
{
    public class MaterialHelper
    {
        public List<Object> FindMaterialForMachine(Chest chest, MachineConfig machineConfig)
        {
            return chest?.items
                .OfType<Object>()
                .Where(i =>
                    i.quality <= machineConfig.MaxQuality
                    && (machineConfig.IsAcceptableObject(i) || machineConfig.IsAcceptableCategory(i))
                )
                .ToList();
        }

        public Object FindCoal(Chest chest, int amountRequired)
        {
            return chest.items
                .OfType<Object>()
                .FirstOrDefault(i => i.parentSheetIndex == Object.coal && i.Stack >= amountRequired);
        }
    }
}
