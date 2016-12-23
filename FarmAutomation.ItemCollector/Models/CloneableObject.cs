using System;

namespace FarmAutomation.ItemCollector.Models
{
  internal class CloneableObject : ICloneable
  {
    public object Clone()
    {
      return this.MemberwiseClone();
    }
  }
}
