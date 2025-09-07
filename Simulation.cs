using System;
using System.Collections.Generic;

namespace AgentSimulation;

public class Simulation
{
    public List<Agent> Agents { get; set; } = new();
    public Scenario Scenario { get; set; }

    public Simulation(ScenarioDefinition scenarioDefinition, string llmEndpoint = "http://localhost:5000")
    {
        Scenario = new Scenario(scenarioDefinition);
        Agents.Add(new Agent("Alice", "Brave"));
        Agents.Add(new Agent("Bob", "Cautious"));
        Agents.Add(new LLMAgent("Charlie", "Logical", endpoint: llmEndpoint));
    }

    public void Run()
    {
        Console.WriteLine($"=== {Scenario.Name} ===");
        Console.WriteLine(Scenario.Description);
        if (Scenario.WinCondition != null) Console.WriteLine($"Win: {Scenario.WinCondition}");
        if (Scenario.LoseCondition != null) Console.WriteLine($"Lose: {Scenario.LoseCondition}");
        Console.WriteLine();

        int steps = 0;
        while (!Scenario.IsResolved && steps < 50) // Increased max steps for longer scenarios
        {
            Console.WriteLine($"Step {steps + 1} - {Scenario.Time}");
            foreach (var agent in Agents)
            {
                agent.Think(Scenario);
                agent.Act(Scenario);
            }
            Scenario.Update();
            Console.WriteLine($"Life Support: {Scenario.LifeSupport} (Decay: {Scenario.LifeSupportDecay}/step)");
            Console.WriteLine();
            steps++;
        }

        Console.WriteLine("=== SIMULATION END ===");
        if (Scenario.IsResolved)
        {
            Console.WriteLine("✅ Scenario resolved successfully!");
        }
        else
        {
            Console.WriteLine("❌ Failed to resolve scenario.");
        }

        // Show final task status
        Console.WriteLine("\nFinal Task Status:");
        foreach (var task in Scenario.Tasks)
        {
            var status = task.IsCompleted ? "✅" : "❌";
            Console.WriteLine($"{status} {task.Name}: {task.Progress}/{task.RequiredProgress}");
        }

        // Show event log summary
        if (Scenario.EventLog.Any())
        {
            Console.WriteLine("\nEvent Summary:");
            foreach (var eventEntry in Scenario.EventLog)
            {
                Console.WriteLine($"• {eventEntry}");
            }
        }
    }
}
