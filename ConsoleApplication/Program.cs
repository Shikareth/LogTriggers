using ConsoleApplication.Events;

using Microsoft.Extensions.Configuration;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;

using System.Text;

#pragma warning disable CS8601, CS8602, CS8603, CS8604

namespace ConsoleApplication;

public class Program
{
  private const string _title = "LogAnalyzer";
  private static int _currentLine = 0;
  private static int _lastLine = 0;
  public static bool Enabled { get; set; } = true;
  public static WebDriver? Driver { get; set; }
  public static Settings? Settings { get; set; }

  public static void Main(string[] args)
  {
    Console.Title = _title;

    // Setup clean application stop
    Console.CancelKeyPress += (sender, args) =>
    {
      Console.WriteLine($"{args.SpecialKey} pressed. Cleaning up ...");

      // Set the Cancel property to true to prevent the process from terminating.
      args.Cancel = true;
      Program.Enabled = false;
    };

    try
    {
      var appsettings = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json")
        .AddCommandLine(args)
        .Build();

      // Enable throwing error on invalid configuration
      Action<BinderOptions> getOptions = (o) => o.ErrorOnUnknownConfiguration = true;

      Settings = appsettings.GetRequiredSection("Settings").Get<Settings>(getOptions);
      EventManager.Events = appsettings.GetRequiredSection("Events").Get<List<Event>>(getOptions);

      // Build paths
      var logFilePath = Path.Combine([Settings.LogFolder, Settings.LogFile]);

      // Check current log file
      if (!File.Exists(logFilePath))
        throw new Exception($"File does not exist: {logFilePath}");

      // Setup Selenium browser
      Driver = Settings.Browser switch
      {
        Enums.BrowserType.Firefox => new FirefoxDriver(),
        Enums.BrowserType.Chrome => new ChromeDriver(),
        Enums.BrowserType.IE => new InternetExplorerDriver(),
        Enums.BrowserType.Edge => new EdgeDriver(),
        Enums.BrowserType.None => null,
        _ => throw new Exception($"Not supported browser: {Settings.Browser}"),
      };

      if (Driver != null)
      {
        Driver.Manage().Window.Position = new System.Drawing.Point(2000, 0);
        Driver.Manage().Window.Maximize();
        Driver.Navigate().GoToUrl(Settings.BrowserEntrySite);
      }

      // Read log file
      using (var fs = File.Open(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      {
        using (var sr = new StreamReader(fs, Encoding.Default))
        {
          var fileWasUpdated = false;

          while (Enabled)
          {
            // Check if file has regenerated -> is shorter then last time
            if (fs.Position > fs.Length)
            {
              var sb = new StringBuilder();

              var msg = "File was regenerated! Reading from start...";
              sb.AppendLine(new string('*', msg.Length));
              sb.AppendLine(msg);
              sb.AppendLine(new string('*', msg.Length));

              ConsoleWrite(sb.ToString(), ConsoleColor.Yellow, ConsoleColor.Black);

              fs.Seek(0, SeekOrigin.Begin);
            }

            // Read log file
            while (!sr.EndOfStream)
            {
              var line = sr.ReadLine();
              EventManager.Parse(line, ++_currentLine);

              fileWasUpdated = true;
            }

            // Act on fulfilled triggers - TODO: from config as abstraction
            var layerChanged = EventManager.EventsBuffered.LastOrDefault(x => x.Label == "LayerChanged");
            if (layerChanged != null && !layerChanged.Consumed)
            {
              layerChanged.Consume();
            }

            // Loop status
            if (fileWasUpdated)
            {
              Console.Title = $"{_title} [{_currentLine}:+{_currentLine - _lastLine}]";
              _lastLine = _currentLine;
            }

            // Deleay read to save resources
            Thread.Sleep(Settings.UpdateDelay);
          }
        }
      }

    }
    catch (Exception ex)
    {
      Error(ParseException(ex));
    }
    finally
    {
      //Driver?.Quit();
      Info("Press any key to exit ...");
    }
  }

  public static void Error(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.Red)
  {
    if (Settings.LogLevel >= Enums.LogLevel.ERROR)
      ConsoleWrite("[ERROR] " + text, bgcolor, fgcolor);
  }
  public static void Warn(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.Yellow)
  {
    if (Settings.LogLevel >= Enums.LogLevel.WARN)
      ConsoleWrite("[WARN] " + text, bgcolor, fgcolor);
  }
  public static void Info(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.White)
  {
    if (Settings.LogLevel >= Enums.LogLevel.INFO)
      ConsoleWrite("[INFO] " + text, bgcolor, fgcolor);
  }
  public static void Debug(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.Green)
  {
    if (Settings.LogLevel >= Enums.LogLevel.DEBUG)
      ConsoleWrite("[DEBUG] " + text, bgcolor, fgcolor);
  }
  private static void ConsoleWrite(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.White)
  {
    var lastBG = Console.BackgroundColor;
    var lastFG = Console.ForegroundColor;

    Console.BackgroundColor = bgcolor;
    Console.ForegroundColor = fgcolor;

    Console.WriteLine(text);

    Console.BackgroundColor = lastBG;
    Console.ForegroundColor = lastFG;
  }
  private static string ParseException(Exception? ex)
  {
    var report = new StringBuilder();

    if (ex == null)
      return "******************** END ********************";

    report.AppendLine(ex.Source);
    report.AppendLine(ex.Message);
    report.AppendLine(ex.StackTrace);
    report.AppendLine(ParseException(ex.InnerException));

    return report.ToString();
  }
}

