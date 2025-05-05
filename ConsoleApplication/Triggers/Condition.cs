using ConsoleApplication.Enums;
using ConsoleApplication.Events;

using System.Text.RegularExpressions;

namespace ConsoleApplication.Triggers
{
  public class Condition : ICloneable
  {
    public required string Label { get; set; }
    public required Config Config { get; set; }

    public AdditionalOptions Options { get; set; } = AdditionalOptions.MatchCase;

    public bool Satisfied { get; private set; } = false;

    public bool CheckCondition(string line)
    {
      if(string.IsNullOrEmpty(Config?.Value))
        throw new Exception($"{nameof(Condition)} matching value cannot be null or empty");

      Satisfied = Config.Mode switch
      {
        MatchingType.Simple => line.Contains(Config.Value),
        MatchingType.Regex => new Regex(Config.Value, RegexOptions.Singleline).IsMatch(line),
        MatchingType.Event => EventManager.EventsBuffered.Any(e => e.Label == Config.Value && e.ConditionsSatisfied && !e.Consumed),
        _ => throw new Exception($"Mode: {Config.Mode} not supported"),
      };

      return Satisfied;
    }

    public void ResetConditionState()
    {
      Satisfied = false;
    }
    public Object Clone()
    {
      return MemberwiseClone();
    }
  }
}