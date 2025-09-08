using System;
using System.Collections.Generic;
using AgentSimulation.Agents;
using AgentSimulation.Scenarios;

namespace AgentSimulation.Core;

public class Simulation
{
    public List<Agent> Agents { get; set; } = new();
    public Scenario Scenario { get; set; }

    public Simulation(ScenarioDefinition scenarioDefinition, string llmEndpoint = "http://localhost:5000", int seed = -1)
    {
        Scenario = new Scenario(scenarioDefinition, seed);
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
        while (steps < 50) // Continue until max steps
        {
            Console.WriteLine($"Step {steps + 1} - {Scenario.Time}");

            // Check if mission is successful before continuing
            if (Scenario.IsSuccessful)
            {
                Console.WriteLine("🎉 Mission accomplished! All tasks completed!");
                break;
            }

            // Agents continue to act
            foreach (var agent in Agents)
            {
                agent.Think(Scenario);
                agent.Act(Scenario);
            }

            Scenario.Update();

            // Show life support status with maintenance task information
            var decayDisplay = Scenario.ActualLifeSupportDecay != Scenario.LifeSupportDecay
                ? $"{Scenario.ActualLifeSupportDecay}/step (reduced from {Scenario.LifeSupportDecay})"
                : $"{Scenario.LifeSupportDecay}/step";

            var lifeSupportTaskInfo = "";
            if (Scenario.HasCompletedLifeSupportTasks())
            {
                var completedCount = Scenario.CompletedLifeSupportTaskCount();
                var totalMaintenance = Scenario.GetLifeSupportTasks().Count;
                var completionRate = Scenario.LifeSupportTaskCompletionRate() * 100;
                lifeSupportTaskInfo = $" [Maintenance: {completedCount}/{totalMaintenance} ({completionRate:F0}%)]";
            }

            Console.WriteLine($"Life Support: {Scenario.LifeSupport} (Decay: {decayDisplay}){lifeSupportTaskInfo}");
            Console.WriteLine($"Colony Stats: {Scenario.ColonyStats.GetStatusSummary()}");

            // Check if mission has failed after the update
            if (Scenario.HasFailed)
            {
                Console.WriteLine("💀 Mission failed! Life support has been depleted!");
                break;
            }
            Console.WriteLine();
            steps++;
        }

        Console.WriteLine("=== SIMULATION END ===");
        if (Scenario.IsSuccessful)
        {
            Console.WriteLine("✅ Scenario resolved successfully!");
        }
        else if (Scenario.HasFailed)
        {
            Console.WriteLine("❌ Mission failed - Life support critical!");
        }
        else
        {
            Console.WriteLine("❌ Failed to resolve scenario within time limit.");
        }

        // Show life support maintenance summary
        var lifeSupportTasks = Scenario.GetLifeSupportTasks();
        if (lifeSupportTasks.Any())
        {
            Console.WriteLine("\nLife Support Maintenance Summary:");
            Console.WriteLine($"Maintenance Tasks Completed: {Scenario.CompletedLifeSupportTaskCount()}/{lifeSupportTasks.Count}");
            Console.WriteLine($"Completion Rate: {Scenario.LifeSupportTaskCompletionRate() * 100:F1}%");
            Console.WriteLine($"Final Life Support: {Scenario.LifeSupport}");
            Console.WriteLine($"Final Decay Rate: {Scenario.ActualLifeSupportDecay}/step (Base: {Scenario.LifeSupportDecay})");
        }

        // Show final task status
        Console.WriteLine("\nFinal Task Status:");
        foreach (var task in Scenario.Tasks)
        {
            var status = task.IsCompleted ? "✅" : "❌";
            Console.WriteLine($"{status} {task.Name} ({task.Type}): {task.Progress}/{task.RequiredProgress}");
        }

        // Show detailed event summary
        if (Scenario.DetailedEventLog.Any())
        {
            Console.WriteLine("\nEvent Summary:");
            foreach (var eventEntry in Scenario.DetailedEventLog)
            {
                Console.WriteLine($"• {eventEntry.TimeStamp} - {eventEntry.EventName}");
                Console.WriteLine($"  {eventEntry.EventDescription}");
                if (eventEntry.Effects.Any())
                {
                    Console.WriteLine("  Effects:");
                    foreach (var effect in eventEntry.Effects)
                    {
                        Console.WriteLine($"    - {effect}");
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
