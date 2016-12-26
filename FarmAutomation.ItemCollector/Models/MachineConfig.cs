using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using StardewValley;

namespace FarmAutomation.ItemCollector.Models
{
    public class MachineConfig
    {
        public MachineConfig()
        {
            AcceptableObjects = new List<AcceptableObject>();
            AcceptableCategories = new List<AcceptableCategory>();
        }

        public MachineConfig(string name, bool insertItems = false, bool collectFrom = true, bool allowConnection = true, int maxQuality = 4)
        {
            Name = name;
            InsertItems = insertItems;
            CollectFrom = collectFrom;
            AllowConnection = allowConnection;
            MaxQuality = maxQuality;
        }

        public string Name { get; set; }
        
        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int CoalRequired { get; set; }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool CollectFrom { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool InsertItems { get; set; }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool AllowConnection { get; set; }

        [DefaultValue(4)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int MaxQuality { get; set; }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<AcceptableObject> AcceptableObjects { get; set; }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<AcceptableCategory> AcceptableCategories { get; set; }

        public bool IsAcceptableObject(Item item)
        {
            var ret = false;

            if (AcceptableObjects != null)
            {
                ret = AcceptableObjects.Any(
                    t => ((t.Index == item.parentSheetIndex && string.IsNullOrWhiteSpace(t.Name)) || t.Name == item.Name)
                         && item.Stack >= t.AmountRequired);
            }

            return ret;
        }

        public bool IsAcceptableCategory(Item item)
        {
            var ret = false;

            if (AcceptableCategories != null)
            {
                ret = AcceptableCategories.Any(t => t.Index == item.category && item.Stack >= t.AmountRequired);
            }

            return ret;
        }
    }
}    
