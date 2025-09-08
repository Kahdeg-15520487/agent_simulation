using System;
using System.Collections.Generic;
using AgentSimulation.Agents;
using AgentSimulation.Scenarios;

namespace AgentSimulation.Core;

public class Simulation
{
    public List<Agent> Agents { get; set; } = new();
    public Scenario Scenario { get; set; }
    public int CurrentStep { get; private set; } = 0;
    public int MaxSteps { get; set; } = 50;
    public bool IsRunning { get; private set; } = false;
    public bool IsCompleted { get; private set; } = false;

    public Simulation(ScenarioDefinition scenarioDefinition, List<Agent> agents, string llmEndpoint = "http://localhost:5000", int seed = -1)
    {
        Scenario = new Scenario(scenarioDefinition, seed);
        Agents = agents;
    }

    // Legacy constructor for backward compatibility
    public Simulation(ScenarioDefinition scenarioDefinition, string llmEndpoint = "http://localhost:5000", int seed = -1, bool includeHuman = false)
    {
        Scenario = new Scenario(scenarioDefinition, seed);
        
        if (includeHuman)
        {
            Agents.Add(new HumanAgent("Player"));
            Agents.Add(new Agent("Alice", "Brave"));
            Agents.Add(new Agent("Bob", "Cautious"));
        }
        else
        {
            Agents.Add(new Agent("Alice", "Brave"));
            Agents.Add(new Agent("Bob", "Cautious"));
            Agents.Add(new LLMAgent("Charlie", "Logical", endpoint: llmEndpoint));
        }
    }

    public void Start()
    {
        if (IsRunning) return;
        
        IsRunning = true;
        IsCompleted = false;
        CurrentStep = 0;
        
        Console.WriteLine($"=== {Scenario.Name} ===");
        Console.WriteLine(Scenario.Description);
        if (Scenario.WinCondition != null) Console.WriteLine($"Win: {Scenario.WinCondition}");
        if (Scenario.LoseCondition != null) Console.WriteLine($"Lose: {Scenario.LoseCondition}");
        Console.WriteLine();
        
        // Show team composition
        Console.WriteLine("👥 TEAM ROSTER");
        Console.WriteLine("=============");
        for (int i = 0; i < Agents.Count; i++)
        {
            var agent = Agents[i];
            var agentType = agent switch
            {
                HumanAgent => "🎮 Human Player",
                LLMAgent => "🧠 AI Assistant",
                _ => "🤖 Basic AI"
            };
            Console.WriteLine($"{i + 1}. {agent.Name} - {agentType} ({agent.Personality})");
        }
        Console.WriteLine();
    }

    public bool ExecuteStep()
    {
        if (!IsRunning || IsCompleted) return false;
        
        CurrentStep++;
        Console.WriteLine($"Step {CurrentStep} - {Scenario.Time}");

        // Check if mission is successful before continuing
        if (Scenario.IsSuccessful)
        {
            Console.WriteLine("🎉 Mission accomplished! All tasks completed!");
            CompleteSimulation();
            return false;
        }

        // Agents act in sequence
        foreach (var agent in Agents)
        {
            agent.Think(Scenario);
            agent.Act(Scenario);
            
            // Add spacing after human player turn for better readability
            if (agent is HumanAgent)
            {
                Console.WriteLine(new string('=', 50));
            }
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
            CompleteSimulation();
            return false;
        }

        // Check if we've reached max steps
        if (CurrentStep >= MaxSteps)
        {
            Console.WriteLine("❌ Failed to resolve scenario within time limit.");
            CompleteSimulation();
            return false;
        }

        Console.WriteLine();
        return true;
    }

    private void CompleteSimulation()
    {
        IsRunning = false;
        IsCompleted = true;
        
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

        ShowFinalSummary();
    }

    private void ShowFinalSummary()
    {
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

    public void Stop()
    {
        IsRunning = false;
        Console.WriteLine("Simulation stopped by user.");
    }

    // Legacy method for backward compatibility
    public void Run()
    {
        Start();
        while (ExecuteStep())
        {
            // Continue stepping until completion
        }
    }
}
