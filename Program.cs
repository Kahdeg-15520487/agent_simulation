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

        // Default to crashed spaceship scenario
        var scenarioDefinition = ScenarioLibrary.GetCrashedSpaceshipScenario();

        // Uncomment to use different scenarios:
        // var scenarioDefinition = ScenarioLibrary.GetZombieApocalypseScenario();
        // var scenarioDefinition = ScenarioLibrary.GetSpaceStationScenario();

        var sim = new Simulation(scenarioDefinition);
        sim.Run();
    }
}
