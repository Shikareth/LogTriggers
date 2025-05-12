using System.Text.Json.Serialization;

using LogAnalyzer.Actions;
using LogAnalyzer.Triggers;

namespace LogAnalyzer.Events
{
  public class LAEvent : ICloneable
  {
    public required string Label { get; set; }
    public List<LACondition> Conditions { get; set; } = [];
    public List<LAAction> Actions { get; set; } = [];
    public List<string> SourceLogs { get; set; } = [];
    public bool Enabled { get; set; } = true;
    public bool Ordered { get; set; } = false;
    public bool Oneshot { get; set; } = false;
    public bool Consumed { get; private set; } = false;
    public bool ConditionsSatisfied { get; private set; } = false;

    [JsonIgnore]
    public int CurrentCondition { get; private set; } = 0;
    [JsonIgnore]
    public int SatisfiedAtLine { get; private set; }

    public string Line { get; private set; } = string.Empty;
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

    public bool CheckConditions(string line, int lineIndex)
    {
      // Got any conditions?
      if(Conditions == null || Conditions.Count <= 0)
        return false;

      // Triggered oneshot will not trigger again >:[
      if(ConditionsSatisfied && Oneshot)
        return false;

      // Reset object state
      if(ConditionsSatisfied)
        ResetEventState();

      if(Ordered)
        if(Conditions[CurrentCondition].CheckCondition(line))
          if(AllConditionsSatisfied()){
            SatisfiedAtLine = lineIndex;
            Line = (string)line.Clone();
          }
          else
            CurrentCondition++;
      
      if(!Ordered)
      {
        foreach(var condition in Conditions)
        {
          if(condition.Satisfied)
            continue;

          condition.CheckCondition(line);
        }

        if(AllConditionsSatisfied()){
          SatisfiedAtLine = lineIndex;
          Line = (string)line.Clone();
        }
      }

      if (ConditionsSatisfied)
        foreach (var action in Actions)
          action.Execute();

      return ConditionsSatisfied;
    }
    public void Consume()
    {
      Consumed = true;
    }

    private void ResetEventState()
    {
      ConditionsSatisfied = false;
      CurrentCondition = 0;

      foreach(var c in Conditions)
        c.ResetConditionState();
    }
    private bool AllConditionsSatisfied()
    {
      return ConditionsSatisfied = Conditions.All(c => c.Satisfied);
    }

    public override string ToString()
    {
      var label = Label + new string(' ', LAEventManager._maxEventLabelLength - Label.Length);
      return $"{label} @ {SatisfiedAtLine,6}: {Line}";
    }

    public object Clone()
    {
      return MemberwiseClone();
    }
  }
}