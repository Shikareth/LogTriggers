using ConsoleApplication.Triggers;
using System.Text.RegularExpressions;

namespace Tests
{
  public class Tests
  {
    [SetUp]
    public void Setup()
    {
    }

    [TestCase("[2025.04.20-15.57.32:320][ 58]LogSquad: Success to load FactionSetup RGF_LO_Motorized for team 1 !", "RGF", "RGF_LO_Motorized")]
    [TestCase("[2025.04.20-15.57.30:325][996]LogSquad: Success to load FactionSetup IMF_LO_LightInfantry for team 2 !", "IMF", "IMF_LO_LightInfantry")]
    [TestCase("[2025.04.20-15.57.30:325][996]LogSquad: Success to load FactionSetup USMC_LO_CombinedArms-Boats for team 2 !", "USMC", "USMC_LO_CombinedArms-Boats")]
    public void Regex_ChangeTeamFaction(string line, string faction, string unit)
    {
      var regex = new Regex("FactionSetup\\s(?=(?<faction>[a-zA-Z0-9]+))(?<unit>(\\S+)).*team\\s(?<team>\\d+)", RegexOptions.ExplicitCapture);

      var match = regex.Matches(line).FirstOrDefault();
      
      Assert.That(match, Is.Not.Null);
      Assert.Multiple(() =>
      {
        Assert.That(match.Groups["faction"].Value, Is.EqualTo(faction));
        Assert.That(match.Groups["unit"].Value, Is.EqualTo(unit));
      });
    }
  }
}
