using System.ComponentModel;
using Newtonsoft.Json;

namespace FarmAutomation.ItemCollector.Models
{
  public class AcceptableObject
  {
    [DefaultValue(null)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public string Name { get; set; }

    [DefaultValue(-1)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public int Index { get; set; }

    [DefaultValue(1)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public int AmountRequired { get; set; }

    public AcceptableObject()
    {
    }

    public AcceptableObject(string name, int amountRequired = 1)
    {
      Name = name;
      AmountRequired = amountRequired;
    }

    public AcceptableObject(int index, int amountRequired = 1)
    {
      Index = index;
      AmountRequired = amountRequired;
    }
  }
}
