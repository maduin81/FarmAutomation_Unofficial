using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace FarmAutomation.ItemCollector.Models
{
  public class MachineConfig
  {
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
  }
}
