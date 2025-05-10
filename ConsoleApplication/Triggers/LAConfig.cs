using LogAnalyzer.Enums;

namespace LogAnalyzer.Triggers
{

  public class LAConfig : ICloneable
  {
    public required MatchingType Mode { get; set; }
    public required string Value { get; set; }

    public Object Clone()
    {
      return MemberwiseClone();
    }
  }
}