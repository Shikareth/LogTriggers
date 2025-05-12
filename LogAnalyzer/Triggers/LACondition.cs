using LogAnalyzer.Enums;
using LogAnalyzer.Events;

using System.Text.RegularExpressions;

namespace LogAnalyzer.Triggers;

public class LACondition : ICloneable
{
  public required string Label { get; set; }
  public required LAConfig Config { get; set; }

  public AdditionalOptions Options { get; set; } = AdditionalOptions.MatchCase;
  public List<LAVariable> Variables { get; set; } = [];

  public bool Satisfied { get; private set; } = false;

  public bool CheckCondition(string line)
  {
    if (Config.Mode != MatchingType.AutoTrigger && string.IsNullOrEmpty(Config?.Value))
      throw new Exception($"{nameof(LACondition)} matching value cannot be null or empty");

    Satisfied = Config.Mode switch
    {
      MatchingType.Simple => line.Contains(Config.Value),
      MatchingType.Regex => new Regex(Config.Value, RegexOptions.Singleline).IsMatch(line),
      MatchingType.Event => LAEventManager.EventsBuffered.Any(e => e.Label == Config.Value && e.ConditionsSatisfied && !e.Consumed),
      MatchingType.AutoTrigger => true,
      _ => throw new Exception($"Mode: {Config.Mode} not supported"),
    };

    if (Satisfied)
      foreach (var variable in Variables)
        variable.Parse(line);

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

  public override string ToString()
  {
    return Label + (Satisfied ? "Satisfied" : "Pending");
  }
}
