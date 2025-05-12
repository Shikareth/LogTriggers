using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

namespace LogAnalyzer.Actions;

public class LAAction
{
  [JsonIgnore]
  public TimeSpan Timeout { get; set; } = new(0, 0, 30);

  public required string Label { get; set; }
  public required string Command { get; set; }
  public bool Quiet { get; set; } = false;
  public List<string>? Arguments { get; set; }


  public void Execute()
  {
    try
    {
      var proc = Process.Start(
        new ProcessStartInfo(Command, Arguments ?? [])
        {
          CreateNoWindow = !Quiet,
          RedirectStandardOutput = false,
          WindowStyle = Quiet ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
        }
      );

      if (proc == null)
        ArgumentNullException.ThrowIfNull(proc, nameof(proc));

      if (proc.WaitForExit(Timeout))
        LogAnalyzer.Debug($"{this} finished with exit code {proc.ExitCode}");
    }
    catch (Exception ex)
    {
      LogAnalyzer.Error(LogAnalyzer.ParseException(ex));
    }
  }

  public override string ToString()
  {
    return $"{Label}[{Timeout}] = {Command} {((Arguments == null) ? string.Empty : new StringBuilder().AppendJoin(" /", Arguments))}";
  }
}

