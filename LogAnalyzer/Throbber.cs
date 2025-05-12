namespace LogAnalyzer;

public partial class LogAnalyzer
{
  public class Throbber(string[] characters)
  {
    public string[] Characters { get; set; } = characters;
    private int _counter = 0;

    public string Next()
    {
      if (_counter >= Characters.Length)
        _counter = 0;

      return Characters[_counter++];
    }
    public string Previous()
    {
      if (_counter < 0)
        _counter = Characters.Length - 1;

      return Characters[_counter--];
    }
    public void Reset()
    {
      _counter = 0;
    }
  }
}


