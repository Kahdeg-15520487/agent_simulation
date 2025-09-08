using System;
using AgentSimulation.Scenarios;

namespace AgentSimulation.Core;

public static class ScenarioSelector
{
    public static ScenarioDefinition SelectScenario()
    {
        Console.WriteLine("🌍 SCENARIO SELECTION");
        Console.WriteLine("===================");
        Console.WriteLine("Choose your survival scenario:");
        Console.WriteLine();
        
        var scenarios = ScenarioLibrary.GetAllScenarios();
        for (int i = 0; i < scenarios.Count; i++)
        {
            var scenario = scenarios[i];
            Console.WriteLine($"{i + 1}. 📖 {scenario.Name}");
            Console.WriteLine($"   {scenario.Description}");
            Console.WriteLine($"   🎯 Win: {scenario.WinCondition}");
            Console.WriteLine($"   💀 Lose: {scenario.LoseCondition}");
            Console.WriteLine($"   ⏱️  {scenario.HoursPerStep} hours per step");
            Console.WriteLine();
        }
        
        while (true)
        {
            Console.Write($"Enter your choice (1-{scenarios.Count}): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= scenarios.Count)
            {
                var selectedScenario = scenarios[choice - 1];
                Console.WriteLine($"✅ Selected: {selectedScenario.Name}");
                Console.WriteLine();
                return selectedScenario;
            }
            Console.WriteLine($"❌ Invalid choice. Please enter a number between 1 and {scenarios.Count}.");
        }
    }
}
