using System.Runtime.CompilerServices;

namespace ConsoleApplication.Triggers
{
  public class Action
  {
    public required string Label { get; set; }
    public List<string>? Arguments { get; set; }
    public System.Action Run { get; set; }
  }
}
