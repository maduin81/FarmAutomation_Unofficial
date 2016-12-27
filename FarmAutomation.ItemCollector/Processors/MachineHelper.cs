using System;
using System.Linq;
using FarmAutomation.Common;
using FarmAutomation.ItemCollector.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace FarmAutomation.ItemCollector.Processors
{
    public static class MachineHelper
    {
        public const int ChestMaxItems = 36;

        private static GhostFarmer _who;

        public static GhostFarmer Who => _who ?? (_who = GhostFarmer.CreateFarmer());

        public static IMonitor Monitor { get; set; }

        public static void DailyReset()
        {
            _who = null;
        }

        public static void ProcessMachine(Object machine, Chest connectedChest, MaterialHelper materialHelper, MachineConfig machineConfig)
        {
            if (machineConfig != null)
            {
                if (connectedChest.items.Any(i => i == null))
                {
                    connectedChest.items.RemoveAll(i => i == null);
                }

                if (MachineIsReadyForHarvest(machine))
                {
                    if (connectedChest.items.Count >= ChestMaxItems && !connectedChest.items.Any(i =>
                    {
                        var o = i as Object;
                        return o != null && (machine.heldObject.parentSheetIndex == i.parentSheetIndex &&
                                                       o.quality == machine.heldObject.quality &&
                                                       machine.heldObject.stack <= i.getRemainingStackSpace());
                    }))
                    {
                        Monitor.Log($"Your Chest in is already full, can't process the {machine.Name} as the item would get lost.", LogLevel.Error);
                        return;
                    }

                    HandleFinishedObjectInMachine(machine, connectedChest);
                }

                if (MachineIsReadyForProcessing(machine))
                {                    
                    var refillables = materialHelper.FindMaterialForMachine(connectedChest, machineConfig);

                    Object coal = null;

                    if (machineConfig.CoalRequired > 0)
                    {
                        coal = materialHelper.FindCoal(connectedChest, machineConfig.CoalRequired);
                        if (coal == null)
                        {
                            // no coal to power the furnace
                            return;
                        }
                    }

                    if (refillables.Any())
                    {
                        if (machineConfig.CoalRequired > 0)
                        {
                            var coalAmount = machineConfig.CoalRequired;
                            MoveItemToFarmer(coal, connectedChest, Who, coalAmount);
                        }

                        foreach (var refillable in refillables)
                        {                            
                            var materialAmount = 1;
                            var message = "OBJECT NOT FOUND";
                            try
                            {
                                object found = null;
                                if (machineConfig.AcceptableObjects != null)
                                {
                                    found = machineConfig.AcceptableObjects.FirstOrDefault(x => (x.Index == refillable.parentSheetIndex && string.IsNullOrWhiteSpace(x.Name)) || x.Name == refillable.Name);
                                }

                                if (found != null)
                                {
                                    materialAmount = ((AcceptableObject)found).AmountRequired;
                                }
                                else
                                {
                                    if (machineConfig.AcceptableCategories != null)
                                    {
                                        message = "CATEGORY NOT FOUND";
                                        found = machineConfig.AcceptableCategories.FirstOrDefault(x => x.Index == refillable.category);
                                        if (found != null)
                                        {
                                            materialAmount = ((AcceptableCategory)found).AmountRequired;
                                        }
                                    }
                                }

                                if (materialAmount <= refillable.Stack)
                                {
                                    var tempRefillable = MoveItemToFarmer(refillable, connectedChest, Who, materialAmount);

                                    if (!PutItemInMachine(machine, tempRefillable, Who))
                                    {
                                        if (machineConfig.Name == "Seed Maker")
                                        {
                                            Monitor.Log("MACHINE ERROR: " + machineConfig.Name + " does not accept " + materialAmount + " items of type: " + tempRefillable.name + ".  Removing from list of Acceptable Objects.", LogLevel.Info);
                                            var badObject = machineConfig.AcceptableObjects.FirstOrDefault(t => t.Index == tempRefillable.parentSheetIndex || t.Name == tempRefillable.name);
                                            if (badObject != null)
                                            {
                                                machineConfig.AcceptableObjects.Remove(badObject);
                                            }
                                            else
                                            {
                                                Monitor.Log("MACHINE ERROR: " + tempRefillable.name + " not found, possible issue with category of items.  Update the ItemCollectorConfiguration.json file and correct the categories for this machine.  Category: " + tempRefillable.category, LogLevel.Error);
                                            }
                                        }

                                        // item was not accepted by the machine, transfer it back to the chest
                                        Who.items.ForEach(i => connectedChest.addItem(i));
                                    }
                                    else
                                    {
                                        Monitor.Log($"Refilled your {machine.Name} with a {refillable.Name} of {(ItemQuality)refillable.quality} quality. The machine now takes {machine.minutesUntilReady} minutes to process. You have {refillable.Stack} {refillable.Name} left", LogLevel.Info);                                        
                                    }

                                    Who.ClearInventory();
                                    return;
                                }                                
                            }
                            catch (Exception e)
                            {
                                Monitor.Log(message, LogLevel.Error);
                                Monitor.Log("MACHINE: " + machineConfig.Name, LogLevel.Error);
                                machineConfig.AcceptableObjects.ForEach(x => Monitor.Log("OBJECT: " + x.Index + " | " + x.Name + " | " + x.AmountRequired, LogLevel.Error));
                                Monitor.Log(e.Message, LogLevel.Error);
                            }                            
                        }
                    }
                }
            }
        }

        public static Object MoveItemToFarmer(Object itemToMove, Chest sourceChest, Farmer target, int amount)
        {
            var temporaryItem = (Object)itemToMove.getOne();
            temporaryItem.Stack = amount;
            var freeIndex = target.items.IndexOf(null);
            target.items[freeIndex] = temporaryItem;
            ItemHelper.RemoveItemFromChest(itemToMove, sourceChest, amount);
            return temporaryItem;
        }

        public static void HandleFinishedObjectInMachine(Object machine, Chest connectedChest)
        {
            var logMessage = $"Collecting a {machine.heldObject?.Name} from your {machine.Name}.";
            if (connectedChest.items.Count > ChestMaxItems)
            {
                Monitor.Log($"Your chest is already full. Cannot place item from {machine.Name} into it.", LogLevel.Error);
                return;
            }

            machine.checkForAction(Who);
            Who.items.ForEach(i =>
            {
                if (i != null)
                {
                    var result = connectedChest.addItem(i);
                    if (result != null)
                    {
                        Game1.player.addItemToInventory(result);
                    }
                }
            });

            if (machine.heldObject != null && machine.minutesUntilReady > 0)
            {
                logMessage += $" The next {machine.heldObject.Name} will be ready in {machine.minutesUntilReady}";
            }

            Who.ClearInventory();
            Monitor.Log(logMessage, LogLevel.Info);
        }

        public static bool MachineIsReadyForHarvest(Object machine)
        {
            return machine.readyForHarvest;
        }

        public static bool MachineIsReadyForProcessing(Object machine)
        {
            return !(machine is Chest) && machine.minutesUntilReady == 0 && machine.heldObject == null;
        }

        public static bool PutItemInMachine(Object machine, Object refillable, Farmer who)
        {
            return machine.performObjectDropInAction(refillable, false, who);
        }
    }
}