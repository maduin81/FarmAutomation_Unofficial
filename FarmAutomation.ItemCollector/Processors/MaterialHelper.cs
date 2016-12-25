using System.Collections.Generic;
using System.Linq;
using FarmAutomation.ItemCollector.Models;
using StardewModdingAPI;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace FarmAutomation.ItemCollector.Processors
{
    public class MaterialHelper
    {
        public List<Object> FindMaterialForMachine(string machineName, Chest chest, MachineConfig machineConfig)
        {
            var ret = chest?.items.Where(i => i is Object 
                && ((Object)i).quality <= machineConfig.MaxQuality
                && (machineConfig.IsAcceptableObject(i)
                || machineConfig.IsAcceptableCategory(i))).Cast<Object>().ToList();

            return ret;
        }

        public Object FindCoal(Chest chest, int amountRequired)
        {
            var ret = chest.items.FirstOrDefault(i => i is Object && i.parentSheetIndex == Object.coal && i.Stack >= amountRequired).Cast<Object>();

            return ret;
        }
    }
}
