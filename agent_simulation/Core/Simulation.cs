using System;
using System.Collections.Generic;
using AgentSimulation.Agents;
using AgentSimulation.Scenarios;

namespace AgentSimulation.Core;

public class Simulation
{
    public List<Agent> Agents { get; set; } = new();

    private TextWriter logWriter;

    public Scenario Scenario { get; set; }
    public int CurrentStep { get; private set; } = 0;
    public int MaxSteps { get; set; } = 50;
    public bool IsRunning { get; private set; } = false;
    public bool IsCompleted { get; private set; } = false;

    public Simulation(ScenarioDefinition scenarioDefinition, List<Agent> agents, TextWriter logWriter, string llmEndpoint = "http://localhost:5000", int seed = -1)
    {
        Scenario = new Scenario(scenarioDefinition, seed);
        Agents = agents;
        this.logWriter = logWriter;
    }

    public void Start()
    {
        if (IsRunning) return;

        IsRunning = true;
        IsCompleted = false;
        CurrentStep = 0;

        logWriter.WriteLine($"=== {Scenario.Name} ===");
        logWriter.WriteLine(Scenario.Description);
        if (Scenario.WinCondition != null) logWriter.WriteLine($"Win: {Scenario.WinCondition}");
        if (Scenario.LoseCondition != null) logWriter.WriteLine($"Lose: {Scenario.LoseCondition}");
        logWriter.WriteLine();

        // Show team composition
        logWriter.WriteLine("👥 TEAM ROSTER");
        logWriter.WriteLine("=============");
        for (int i = 0; i < Agents.Count; i++)
        {
            var agent = Agents[i];
            var agentType = agent switch
            {
                HumanAgent => "🎮 Human Player",
                LLMAgent => "🧠 AI Assistant",
                _ => "🤖 Basic AI"
            };
            logWriter.WriteLine($"{i + 1}. {agent.Name} - {agentType} ({agent.Personality})");
        }
        logWriter.WriteLine();
    }

    public bool ExecuteStep()
    {
        if (!IsRunning || IsCompleted) return false;

        CurrentStep++;
        logWriter.WriteLine($"Step {CurrentStep} - {Scenario.Time}");

        // Check if mission is successful before continuing
        if (Scenario.IsSuccessful)
        {
            logWriter.WriteLine("🎉 Mission accomplished! All tasks completed!");
            CompleteSimulation();
            return false;
        }

        // Agents act in sequence
        foreach (var agent in Agents)
        {
            logWriter.WriteLine($"\n{agent.Name}:");
            logWriter.WriteLine(agent.Think(Scenario));
            logWriter.WriteLine(agent.Act(Scenario));

            // Add spacing after human player turn for better readability
            if (agent is HumanAgent)
            {
                logWriter.WriteLine(new string('=', 50));
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

        logWriter.WriteLine($"Life Support: {Scenario.LifeSupport} (Decay: {decayDisplay}){lifeSupportTaskInfo}");
        logWriter.WriteLine($"Colony Stats: {Scenario.ColonyStats.GetStatusSummary()}");
        logWriter.WriteLine(new string('=', 50));

        // Check if mission has failed after the update
        if (Scenario.HasFailed)
        {
            logWriter.WriteLine("💀 Mission failed! Life support has been depleted!");
            CompleteSimulation();
            return false;
        }

        // Check if we've reached max steps
        if (CurrentStep >= MaxSteps)
        {
            logWriter.WriteLine("❌ Failed to resolve scenario within time limit.");
            CompleteSimulation();
            return false;
        }

        logWriter.WriteLine();
        return true;
    }

    private void CompleteSimulation()
    {
        IsRunning = false;
        IsCompleted = true;

        logWriter.WriteLine("=== SIMULATION END ===");
        if (Scenario.IsSuccessful)
        {
            logWriter.WriteLine("✅ Scenario resolved successfully!");
        }
        else if (Scenario.HasFailed)
        {
            logWriter.WriteLine("❌ Mission failed - Life support critical!");
        }
        else
        {
            logWriter.WriteLine("❌ Failed to resolve scenario within time limit.");
        }

        ShowFinalSummary();
    }

    private void ShowFinalSummary()
    {
        // Show life support maintenance summary
        var lifeSupportTasks = Scenario.GetLifeSupportTasks();
        if (lifeSupportTasks.Any())
        {
            logWriter.WriteLine("\nLife Support Maintenance Summary:");
            logWriter.WriteLine($"Maintenance Tasks Completed: {Scenario.CompletedLifeSupportTaskCount()}/{lifeSupportTasks.Count}");
            logWriter.WriteLine($"Completion Rate: {Scenario.LifeSupportTaskCompletionRate() * 100:F1}%");
            logWriter.WriteLine($"Final Life Support: {Scenario.LifeSupport}");
            logWriter.WriteLine($"Final Decay Rate: {Scenario.ActualLifeSupportDecay}/step (Base: {Scenario.LifeSupportDecay})");
        }

        // Show final task status
        logWriter.WriteLine("\nFinal Task Status:");
        foreach (var task in Scenario.Tasks)
        {
            var status = task.IsCompleted ? "✅" : "❌";
            logWriter.WriteLine($"{status} {task.Name} ({task.Type}): {task.Progress}/{task.RequiredProgress}");
        }

        // Show detailed event summary
        if (Scenario.DetailedEventLog.Any())
        {
            logWriter.WriteLine("\nEvent Summary:");
            foreach (var eventEntry in Scenario.DetailedEventLog)
            {
                logWriter.WriteLine($"• {eventEntry.TimeStamp} - {eventEntry.EventName}");
                logWriter.WriteLine($"  {eventEntry.EventDescription}");
                if (eventEntry.Effects.Any())
                {
                    logWriter.WriteLine("  Effects:");
                    foreach (var effect in eventEntry.Effects)
                    {
                        logWriter.WriteLine($"    - {effect}");
                    }
                }
                logWriter.WriteLine();
            }
        }
    }

    public void Stop()
    {
        IsRunning = false;
        logWriter.WriteLine("Simulation stopped by user.");
    }
}
