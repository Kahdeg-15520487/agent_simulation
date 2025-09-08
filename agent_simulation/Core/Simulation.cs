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
    public List<string> Logs { get; private set; } = new();

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
        }
        Agents.Add(new Agent("Alice", "Brave"));
        Agents.Add(new Agent("Bob", "Cautious"));
        Agents.Add(new LLMAgent("Charlie", "Logical", endpoint: llmEndpoint));
    }

    public void Log(string message)
    {
        Logs.Add(message);
    }

    public void ClearLogs()
    {
        Logs.Clear();
    }

    public void Start()
    {
        if (IsRunning) return;
        
        IsRunning = true;
        IsCompleted = false;
        CurrentStep = 0;
        
        Log($"=== {Scenario.Name} ===");
        Log(Scenario.Description);
        if (Scenario.WinCondition != null) Log($"Win: {Scenario.WinCondition}");
        if (Scenario.LoseCondition != null) Log($"Lose: {Scenario.LoseCondition}");
        Log("");
        
        // Show team composition
        Log("👥 TEAM ROSTER");
        Log("=============");
        for (int i = 0; i < Agents.Count; i++)
        {
            var agent = Agents[i];
            var agentType = agent switch
            {
                HumanAgent => "🎮 Human Player",
                LLMAgent => "🤖 LLM Agent",
                _ => "👤 Basic Agent"
            };
            Log($"{i + 1}. {agent.Name} - {agentType} ({agent.Personality})");
        }
        Log("");
    }

    public bool ExecuteStep()
    {
        if (!IsRunning || IsCompleted) return false;

        CurrentStep++;
        ClearLogs();

        Log($"Step {CurrentStep} - {Scenario.Time}");

        // Check if mission is successful before continuing
        if (Scenario.IsSuccessful)
        {
            Log("🎉 Mission accomplished! All tasks completed!");
            CompleteSimulation();
            return false;
        }

        // Agents act in sequence
        foreach (var agent in Agents)
        {
            agent.Think(Scenario);
            agent.Act(Scenario);
            
            // Collect agent logs
            foreach (var log in agent.Logs)
            {
                Log(log);
            }
            
            // Add spacing after human player turn for better readability
            if (agent is HumanAgent)
            {
                Log(new string('=', 50));
            }
        }

        Scenario.Update();

        // Collect scenario logs
        foreach (var log in Scenario.Logs)
        {
            Log(log);
        }
        Scenario.Logs.Clear();

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

        Log($"Life Support: {Scenario.LifeSupport} (Decay: {decayDisplay}){lifeSupportTaskInfo}");
        Log($"Colony Stats: {Scenario.ColonyStats.GetStatusSummary()}");

        // Check if mission has failed after the update
        if (Scenario.HasFailed)
        {
            Log("💀 Mission failed! Life support has been depleted!");
            CompleteSimulation();
            return false;
        }

        // Check if we've reached max steps
        if (CurrentStep >= MaxSteps)
        {
            Log("❌ Failed to resolve scenario within time limit.");
            CompleteSimulation();
            return false;
        }

        Log("");
        return true;
    }

    private void CompleteSimulation()
    {
        IsRunning = false;
        IsCompleted = true;
        
        Log("=== SIMULATION END ===");
        if (Scenario.IsSuccessful)
        {
            Log("✅ Scenario resolved successfully!");
        }
        else if (Scenario.HasFailed)
        {
            Log("❌ Mission failed - Life support critical!");
        }
        else
        {
            Log("❌ Failed to resolve scenario within time limit.");
        }
    }
}
