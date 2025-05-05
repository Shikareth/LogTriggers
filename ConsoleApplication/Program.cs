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
    public static List<Faction>? Factions { get; set; }

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
        Action<BinderOptions> getOptions = (o)=> o.ErrorOnUnknownConfiguration = true;

        Settings = appsettings.GetRequiredSection("Settings").Get<Settings>(getOptions);
        var mappings  = appsettings.GetRequiredSection("Mappings");
        Factions = mappings.GetRequiredSection("Factions").Get<List<Faction>>(getOptions);
        EventManager.Events = appsettings.GetRequiredSection("Events").Get<List<Event>>(getOptions);

        // Build paths
        var appdataPath = appsettings.GetValue<string>("LOCALAPPDATA");
        var configFilePath = Path.Combine([appdataPath, Settings.ConfigsFolder]);
        var logFilePath = Path.Combine([appdataPath, Settings.LogFolder, Settings.LogFile]);

        //// Look for configuration files - Remove ?
        //var config = new ConfigurationBuilder();
        //var configFiles = Directory.GetFiles(configFilePath, "*.ini", SearchOption.AllDirectories);
        //if(Settings.ConfigurationFiles != null)
        //{
        //  foreach(var file in configFiles)
        //  {
        //    if(Settings.ConfigurationFiles.Contains(Path.GetFileName(file)))
        //      config.AddIniFile(file, true);
        //  }
        //  config.Build();
        //}

        // Check current log file
        if(!File.Exists(logFilePath))
          throw new Exception($"File does not exist: {logFilePath}");

        // Setup Selenium browser
        Driver = Settings.Browser switch
        {
          Enums.BrowserType.Firefox => new FirefoxDriver(),
          Enums.BrowserType.Chrome => new ChromeDriver(),
          Enums.BrowserType.IE => new InternetExplorerDriver(),
          Enums.BrowserType.Edge => new EdgeDriver(),
          _ => throw new Exception($"Not supported browser: {Settings.Browser}"),
        };

        Driver.Manage().Window.Position = new System.Drawing.Point(2000, 0);
        Driver.Manage().Window.Maximize();
        Driver.Navigate().GoToUrl(Settings.SquadMapSite);

        // Read log file
        using(var fs = File.Open(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          using(var sr = new StreamReader(fs, Encoding.Default))
          {
            while(Enabled)
            {
              // Check if file has regenerated -> is shorter then last time
              if(fs.Position > fs.Length)
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
              while(!sr.EndOfStream)
              {
                var line = sr.ReadLine();
                EventManager.Parse(line, ++_currentLine);
              }

              // Act on fulfilled triggers
              var layerChanged = EventManager.EventsBuffered.LastOrDefault(x => x.Label == "LayerChanged");
              if(layerChanged != null && !layerChanged.Consumed)
              {
                ChangeLayer(layerChanged);
                ChangeOptions(true, false, true);

                layerChanged.Consume();
              }

              var updateMap = EventManager.EventsBuffered.LastOrDefault(x => x.Label == "UpdateMap");
              if(updateMap != null && !updateMap.Consumed)
              {
                var loadedFactionUnit_team1 = EventManager.EventsBuffered.LastOrDefault(t => t.Label == "LoadedFactionUnit_team1");
                if(loadedFactionUnit_team1 != null && !loadedFactionUnit_team1.Consumed)
                {
                  ChangeTeamFaction(loadedFactionUnit_team1);
                  loadedFactionUnit_team1.Consume();
                }

                var loadedFactionUnit_team2 = EventManager.EventsBuffered.LastOrDefault(t => t.Label == "LoadedFactionUnit_team2");
                if(loadedFactionUnit_team2 != null && !loadedFactionUnit_team2.Consumed)
                {
                  ChangeTeamFaction(loadedFactionUnit_team2);
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
      catch(Exception ex)
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
      if(Settings.LogLevel >= Enums.LogLevel.ERROR)
        ConsoleWrite("[ERROR] " + text, bgcolor, fgcolor);
    }
    public static void Warn(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.Yellow)
    {
      if(Settings.LogLevel >= Enums.LogLevel.WARN)
        ConsoleWrite("[WARN] " + text, bgcolor, fgcolor);
    }
    public static void Info(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.White)
    {
      if(Settings.LogLevel >= Enums.LogLevel.INFO)
        ConsoleWrite("[INFO] " + text, bgcolor, fgcolor);
    }
    public static void Debug(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.Green)
    {
      if(Settings.LogLevel >= Enums.LogLevel.DEBUG)
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

      if(ex == null)
        return "******************** END ********************";

      report.AppendLine(ex.Source);
      report.AppendLine(ex.Message);
      report.AppendLine(ex.StackTrace);
      report.AppendLine(ParseException(ex.InnerException));

      return report.ToString();
    }

    public static void ChangeLayer(Event trigger)
    {
      try
      {
        // https://squadmaps.com/?map=Harju
        // https://squadmaps.com/map?name=Harju&layer=RAAS%20v1

        // LayerChange @  16152: [2025.04.24-10.24.19:473][501]LogSquad: Success to Determine Startup Level : Yehorivka / Yehorivka RAAS v1
        var regex = new Regex("Startup Level : (?<map>.*)\\s\\/\\s(?<layer>.*)");
        var matches = regex.Matches(trigger.Line);

        if(matches == null || matches.Count <= 0)
        {
          ConsoleWrite("Could not match map/layer", ConsoleColor.DarkBlue, ConsoleColor.Red);
          return;
        }

        var map = matches.First().Groups["map"].Value.Trim().Replace(' ', '+');
        var layer = matches.First().Groups["layer"].Value.Substring(map.Length).Trim();

        // LayerChange @  18890: [2025.04.24-11.13.52:673][104]LogSquad: Success to Determine Startup Level : Black Coast / Black Coast RAAS v2
        // https://squadmaps.com/map?name=Black%20Coast&layer=RAAS%20v2

        // LayerChange @  26711: [2025.04.24-12.09.37:341][357]LogSquad: Success to Determine Startup Level : Gorodok / Gorodok RAAS v1
        // https://squadmaps.com/map?name=Gorodok&layer=RAAS%20v1

        Driver.Navigate().GoToUrl($"{Settings.SquadMapSite}/map?name={map}&layer={layer}");
      }
      catch(Exception ex)
      {
        ParseException(ex);
      }
    }
    public static void ChangeTeamFaction(Event trigger)
    {
      try
      {
        // [2025.04.20-15.57.32:320][ 58]LogSquad: Success to load FactionSetup RGF_LO_Motorized for team 1 !
        // [2025.04.20-15.57.30:325][996]LogSquad: Success to load FactionSetup IMF_LO_LightInfantry for team 2 !
        var regex = new Regex("FactionSetup\\s(?=(?<faction>[a-zA-Z0-9]+))(?<unit>(\\S+)).*team\\s(?<team>\\d+)", RegexOptions.ExplicitCapture);
        var match = regex.Matches(trigger.Line).FirstOrDefault();

        // Have we matched anything?
        if(match == null || match.Groups.Count <= 0)
        {
          Warn("Could not match faction/unit");
          return;
        }

        // Do we have Faction??
        var faction = Factions.FirstOrDefault(f => f.Tag.Contains(match.Groups["faction"].Value));
        if(faction == null)
        {
          Warn("Could not match faction");
          return;
        }

        // Do we have Unit??
        var unit = faction.Units.FirstOrDefault(u => u.Tag.Contains(match.Groups["unit"].Value));
        if(unit == null)
        {
          Warn("Could not match unit");
          return;
        }

        // Do we know which team?
        var team = match.Groups["team"].Value;
        if(team == null)
        {
          Warn("Could not match unit");
          return;
        }

        // Get <span> with team selectors
        var teamElement = Driver
        .FindElement(By.XPath($"//span[contains(@class,'TopPanel_team{team}')]"));
        Debug($"{nameof(teamElement)}::{teamElement}");

        // Activate faction dropdown
        var factionDropdown = teamElement.FindElement(By.XPath($"//span[contains(@class,'TopPanel_team{team}')]//div[contains(@class,'FactionSelect_placeholder')]"));
        Debug($"{nameof(factionDropdown)}::{factionDropdown}");
        factionDropdown.Click();

        // Set faction
        var factionBtn = teamElement
        .FindElements(By.XPath($"//span[contains(@class,'TopPanel_team{team}')]//div[contains(@class,'FactionSelect_option')]"))
        .First(x => x.FindElement(By.TagName("img")).GetDomAttribute("src").Contains(faction.Tag));
        Debug($"{nameof(factionBtn)}::{factionBtn}");
        factionBtn.Click();

        Info($"Changed team{team} faction to {faction}");

        // Activate unit dropdown
        var unitDropdown = teamElement
        .FindElement(By.XPath($"//span[contains(@class,'TopPanel_team{team}')]//span[contains(@class,'factionNameContainer')]"));
        unitDropdown.Click();

        // Select unit
        unitDropdown
          .FindElements(By.XPath($"//span[contains(@class,'TopPanel_team{team}')]//div[contains(@role,'option')]"))
          .FirstOrDefault(o => o.Text == unit.FullName)
          .Click();

        Info($"Changed team{team} unit to {unit}");
      }
      catch(Exception ex)
      {
        Error(ParseException(ex));
      }
    }
    public static void ChangeOptions(bool mainProtectionZonesEnabled, bool mortarsEnabled, bool gridlinesEnabled)
    {
      const string active = "Settings_active";
      void act(IWebElement element, bool enabled)
      {
        if(element.GetAttribute("class").Contains(active) != enabled)
          element.Click();
      }

      var settingsButtons = Driver
        .FindElement(By.XPath($"//div[contains(@class,'Settings_buttonCluster')]"));
      Debug($"{nameof(settingsButtons)}::{settingsButtons}");

      var mainProtectionZones = settingsButtons
        .FindElement(By.XPath($"//button[contains(@title,'Main Protection Zones')]"));
      Debug($"{nameof(mainProtectionZones)}::{mainProtectionZones}");

      var mortars = settingsButtons
        .FindElement(By.XPath($"//button[contains(@title,'Mortars')]"));
      Debug($"{nameof(mortars)}::{mortars}");

      var gridlines = settingsButtons
        .FindElement(By.XPath($"//button[contains(@title,'Gridlines')]"));
      Debug($"{nameof(gridlines)}::{gridlines}");

      act(mainProtectionZones, mainProtectionZonesEnabled);
      act(mortars, mortarsEnabled);
      act(gridlines, gridlinesEnabled);
    }
  }
}
