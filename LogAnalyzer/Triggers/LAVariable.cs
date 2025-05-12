using System.Text.RegularExpressions;

namespace LogAnalyzer.Triggers;

public class LAVariable : ICloneable
{
  public required string Label { get; set; }
  public required string Filter { get; set; }

  public string? Value { get; private set; }


  public void Parse(string line)
  {
    var regex = new Regex(Filter, RegexOptions.Singleline);
    var match = regex.Match(line);

    if (!match.Success)
    {
      LogAnalyzer.Warn($"Could not match value: {Label}");
      return;
    }
    
    Value = match.Groups[match.Groups.Count - 1].Value;
  }

  public object Clone()
  {
    return MemberwiseClone();
  }
  public override string ToString()
  {
    return $"{Label}" + (Value != null ? $"= {Value}" : string.Empty);
  }
}
