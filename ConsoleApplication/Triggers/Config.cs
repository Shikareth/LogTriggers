using ConsoleApplication.Enums;

namespace ConsoleApplication.Triggers
{

  public class Config : ICloneable
  {
    public required MatchingType Mode { get; set; }
    public required string Value { get; set; }

    public Object Clone()
    {
      return MemberwiseClone();
    }
  }
}