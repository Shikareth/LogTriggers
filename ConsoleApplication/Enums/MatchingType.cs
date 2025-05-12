namespace LogAnalyzer.Enums
{
  public enum MatchingType
  {
    None,
    Simple,
    /// <summary>
    /// \n, \r, \t, \0, \x,...
    /// </summary>
    Extended,
    Regex,
    Event,
    AutoTrigger
  }
}