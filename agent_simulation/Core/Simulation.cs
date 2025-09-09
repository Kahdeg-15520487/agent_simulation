using System;
using System.Collections.Generic;
using AgentSimulation.Agents;
using AgentSimulation.Scenarios;
using AgentSimulation.Events;
using AgentSimulation.Tasks;

namespace AgentSimulation.Core;

public class Simulation : SimulationEventPublisher
{
    public List<Agent> Agents { get; set; } = new();

    private TextWriter logWriter;
    public bool IsPaused { get; private set; } = false;

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

        // Fire simulation started event
        OnSimulationStarted(new SimulationStateEventArgs(IsRunning, IsCompleted, IsPaused, "Simulation started"));
        OnLogMessageGenerated(new SimulationLogEventArgs($"=== {Scenario.Name} ==="));

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
        if (!IsRunning || IsCompleted || IsPaused) return false;

        CurrentStep++;
        
        // Fire step started event
        OnStepStarted(new SimulationStepEventArgs(CurrentStep, MaxSteps));
        OnLogMessageGenerated(new SimulationLogEventArgs($"Step {CurrentStep} - {Scenario.Time}"));
        
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
            
            string thought = agent.Think(Scenario);
            logWriter.WriteLine(thought);
            
            string action = agent.Act(Scenario);
            logWriter.WriteLine(action);

            // Fire agent action event
            OnAgentPerformedAction(new AgentActionEventArgs(agent, action));

            // Add spacing after human player turn for better readability
            if (agent is HumanAgent)
            {
                logWriter.WriteLine(new string('=', 50));
            }
        }

        // Check for task progress updates before scenario update
        var tasksBefore = Scenario.Tasks.ToDictionary(t => t.Name, t => t.Progress);

        Scenario.Update();

        // Check for task progress changes and fire events
        foreach (var task in Scenario.Tasks)
        {
            if (tasksBefore.TryGetValue(task.Name, out int previousProgress) && previousProgress != task.Progress)
            {
                OnTaskProgressUpdated(new TaskStatusEventArgs(task, previousProgress, task.Progress));
                
                if (task.IsCompleted && previousProgress < task.RequiredProgress)
                {
                    OnTaskCompleted(new TaskCompletedEventArgs(task));
                }
            }
        }

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
        
        // Fire step completed event
        OnStepCompleted(new SimulationStepEventArgs(CurrentStep, MaxSteps));
        
        return true;
    }

    private void CompleteSimulation()
    {
        IsRunning = false;
        IsCompleted = true;

        // Fire simulation completed event
        string statusMessage = Scenario.IsSuccessful ? "Scenario resolved successfully!" :
                              Scenario.HasFailed ? "Mission failed - Life support critical!" :
                              "Failed to resolve scenario within time limit.";
        
        OnSimulationCompleted(new SimulationStateEventArgs(IsRunning, IsCompleted, IsPaused, statusMessage));

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
        OnSimulationStopped(new SimulationStateEventArgs(IsRunning, IsCompleted, IsPaused, "Simulation stopped by user"));
        OnLogMessageGenerated(new SimulationLogEventArgs("Simulation stopped by user."));
        logWriter.WriteLine("Simulation stopped by user.");
    }

    public void Pause()
    {
        if (!IsRunning || IsPaused) return;
        IsPaused = true;
        OnSimulationPaused(new SimulationStateEventArgs(IsRunning, IsCompleted, IsPaused, "Simulation paused"));
        OnLogMessageGenerated(new SimulationLogEventArgs("Simulation paused."));
    }

    public void Resume()
    {
        if (!IsRunning || !IsPaused) return;
        IsPaused = false;
        OnSimulationResumed(new SimulationStateEventArgs(IsRunning, IsCompleted, IsPaused, "Simulation resumed"));
        OnLogMessageGenerated(new SimulationLogEventArgs("Simulation resumed."));
    }

    /// <summary>
    /// Request user input for a human agent
    /// </summary>
    public async Task<int> RequestUserInputAsync(Agent agent, string prompt, List<string> options)
    {
        var args = new UserInputRequestedEventArgs(agent, prompt, options);
        OnUserInputRequested(args);
        return await args.ResponseTask.Task;
    }

    /// <summary>
    /// Get current task statuses for UI display
    /// </summary>
    public List<AgentSimulation.Events.TaskStatus> GetTaskStatuses()
    {
        return Scenario.Tasks.Select(task => new AgentSimulation.Events.TaskStatus
        {
            Name = task.Name,
            Description = task.Description,
            Progress = task.Progress,
            RequiredProgress = task.RequiredProgress,
            IsCompleted = task.IsCompleted,
            Type = task.Type,
            ProgressPercentage = task.RequiredProgress > 0 ? (double)task.Progress / task.RequiredProgress * 100 : 0
        }).ToList();
    }

    /// <summary>
    /// Get current simulation status for UI display
    /// </summary>
    public SimulationStatus GetSimulationStatus()
    {
        return new SimulationStatus
        {
            CurrentStep = CurrentStep,
            MaxSteps = MaxSteps,
            IsRunning = IsRunning,
            IsCompleted = IsCompleted,
            IsPaused = IsPaused,
            LifeSupport = Scenario.LifeSupport,
            LifeSupportDecay = Scenario.ActualLifeSupportDecay,
            IsSuccessful = Scenario.IsSuccessful,
            HasFailed = Scenario.HasFailed,
            ProgressPercentage = MaxSteps > 0 ? (double)CurrentStep / MaxSteps * 100 : 0
        };
    }
}
