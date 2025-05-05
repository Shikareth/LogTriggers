using ConsoleApplication.SQUAD;
using ConsoleApplication.Triggers;

using Microsoft.Extensions.Configuration;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;

using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable CS8601, CS8602, CS8603, CS8604

namespace ConsoleApplication
{
  public class Program
  {
    private static int _currentLine = 0;
    public static bool Enabled { get; set; } = true;
    public static WebDriver? Driver { get; set; }
    public static Settings? Settings { get; set; }

    public static void Main(string[] args)
    {
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
        var mappings = appsettings.GetRequiredSection("Mappings");
        EventManager.Events = appsettings.GetRequiredSection("Events").Get<List<Event>>(getOptions);

        // Build paths
        var appdataPath = appsettings.GetValue<string>("LOCALAPPDATA");
        var configFilePath = Path.Combine([appdataPath, Settings.ConfigsFolder]);
        var logFilePath = Path.Combine([appdataPath, Settings.LogFolder, Settings.LogFile]);

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
              }

              // Act on fulfilled triggers
              var layerChanged = EventManager.EventsBuffered.LastOrDefault(x => x.Label == "LayerChanged");
              if (layerChanged != null && !layerChanged.Consumed)
              {
                layerChanged.Consume();
              }

              var updateMap = EventManager.EventsBuffered.LastOrDefault(x => x.Label == "UpdateMap");
              if (updateMap != null && !updateMap.Consumed)
              {
                var loadedFactionUnit_team1 = EventManager.EventsBuffered.LastOrDefault(t => t.Label == "LoadedFactionUnit_team1");
                if (loadedFactionUnit_team1 != null && !loadedFactionUnit_team1.Consumed)
                {
                  loadedFactionUnit_team1.Consume();
                }

                var loadedFactionUnit_team2 = EventManager.EventsBuffered.LastOrDefault(t => t.Label == "LoadedFactionUnit_team2");
                if (loadedFactionUnit_team2 != null && !loadedFactionUnit_team2.Consumed)
                {
                  loadedFactionUnit_team2.Consume();
                }

                updateMap.Consume();
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
}
