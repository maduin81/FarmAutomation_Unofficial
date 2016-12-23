using System.ComponentModel;
using Newtonsoft.Json;

namespace FarmAutomation.ItemCollector.Models
{
  public class AcceptableCategory
  {
    public int Index { get; set; }

    [DefaultValue(1)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public int AmountRequired { get; set; }

    public AcceptableCategory(int index)
    {
      Index = index;
      AmountRequired = 1;
    }
  }
}
