using AgentSimulation.Core;
using AgentSimulation.Scenarios;

namespace AgentSimulation;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Available Scenarios:");
        var scenarios = ScenarioLibrary.GetAllScenarios();
        for (int i = 0; i < scenarios.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {scenarios[i].Name}");
            Console.WriteLine($"   {scenarios[i].Description}");
        }
        Console.WriteLine();

        // Default to zombie apocalypse scenario to test the enhanced version
        var scenarioDefinition = ScenarioLibrary.GetZombieApocalypseScenario();

        // Uncomment to use different scenarios:
        // var scenarioDefinition = ScenarioLibrary.GetCrashedSpaceshipScenario();
        // var scenarioDefinition = ScenarioLibrary.GetSpaceStationScenario();

        var sim = new Simulation(scenarioDefinition, "http://localhost:8080", 0);
        sim.Run();
    }
}
