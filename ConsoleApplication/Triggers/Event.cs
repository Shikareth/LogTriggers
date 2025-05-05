namespace ConsoleApplication.Triggers
{
  public class Event : ICloneable
  {
    public required string Label { get; set; }
    public List<Condition> Conditions { get; set; } = [];
    public List<Action> Actions { get; set; } = [];
    public bool Enabled { get; set; } = true;
    public bool Ordered { get; set; } = false;
    public bool Consumed { get; private set; } = false;
    public bool ConditionsSatisfied { get; private set; } = false;

    public int CurrentCondition { get; private set; } = 0;
    public int SatisfiedAtLine { get; private set; }

    public string Line { get; private set; } = string.Empty;
    public ConsoleColor Color { get; set; } = ConsoleColor.White;

    public bool CheckConditions(string line, int lineIndex)
    {
      // Got any conditions?
      if(Conditions == null || Conditions.Count <= 0)
        return false;

      // Reset object state
      if(ConditionsSatisfied)
        ResetEventState();

      if(Ordered)
      {
        if(Conditions[CurrentCondition].CheckCondition(line))
        {
          if(AllConditionsSatisfied()){
            SatisfiedAtLine = lineIndex;
            Line = (string)line.Clone();
          }
          else
            CurrentCondition++;
        }
      }
      
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

    public override String ToString()
    {
      return $"{Label} @ {SatisfiedAtLine,6}: {Line}";
    }

    public Object Clone()
    {
      return MemberwiseClone();
    }
  }
}