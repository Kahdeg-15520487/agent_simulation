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

    public Simulation(ScenarioDefinition scenarioDefinition, List<Agent> agents, TextWriter logWriter, int seed = -1)
    {
        Scenario = new Scenario(scenarioDefinition, logWriter, seed);
        Agents = agents.Select(a => { a.Simulation = this; return a; }).ToList();
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
        OnLogMessageGenerated(new SimulationLogEventArgs(Scenario.Description));
        logWriter.WriteLine(Scenario.Description);
        if (Scenario.WinCondition != null)
        {
            OnLogMessageGenerated(new SimulationLogEventArgs($"Win: {Scenario.WinCondition}"));
            logWriter.WriteLine($"Win: {Scenario.WinCondition}");
        }
        if (Scenario.LoseCondition != null)
        {
            OnLogMessageGenerated(new SimulationLogEventArgs($"Lose: {Scenario.LoseCondition}"));
            logWriter.WriteLine($"Lose: {Scenario.LoseCondition}");
        }
        OnLogMessageGenerated(new SimulationLogEventArgs(""));
        logWriter.WriteLine();

        // Show team composition
        OnLogMessageGenerated(new SimulationLogEventArgs("👥 TEAM ROSTER"));
        logWriter.WriteLine("👥 TEAM ROSTER");
        OnLogMessageGenerated(new SimulationLogEventArgs("============="));
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
            OnLogMessageGenerated(new SimulationLogEventArgs($"{i + 1}. {agent.Name} - {agentType} ({agent.Personality})"));
            logWriter.WriteLine($"{i + 1}. {agent.Name} - {agentType} ({agent.Personality})");
        }
        OnLogMessageGenerated(new SimulationLogEventArgs(""));
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
            OnLogMessageGenerated(new SimulationLogEventArgs("🎉 Mission accomplished! All tasks completed!"));
            logWriter.WriteLine("🎉 Mission accomplished! All tasks completed!");
            CompleteSimulation();
            return false;
        }

        // Agents act in sequence
        foreach (var agent in Agents)
        {
            OnLogMessageGenerated(new SimulationLogEventArgs($"\n{agent.Name}:"));
            logWriter.WriteLine($"\n{agent.Name}:");

            string thought = agent.Think(Scenario);
            OnLogMessageGenerated(new SimulationLogEventArgs(thought));
            logWriter.WriteLine(thought);

            string action = agent.Act(Scenario, null); // Use GUID-based version with null for random selection
            OnLogMessageGenerated(new SimulationLogEventArgs(action));
            logWriter.WriteLine(action);

            // Fire agent action event
            OnAgentPerformedAction(new AgentActionEventArgs(agent, action));

            // Add spacing after human player turn for better readability
            if (agent is HumanAgent)
            {
                OnLogMessageGenerated(new SimulationLogEventArgs(new string('=', 50)));
                logWriter.WriteLine(new string('=', 50));
            }
        }

        // Check for task progress updates before scenario update
        var tasksBefore = Scenario.Tasks.Select(t => new { Task = t, Name = t.Name, Progress = t.Progress }).ToList();

        Scenario.Update();

        // Check for task progress changes and fire events
        foreach (var task in Scenario.Tasks)
        {
            // Find the corresponding task from before the update by name and check if it exists
            var previousTask = tasksBefore.FirstOrDefault(tb => tb.Name == task.Name && tb.Task == task);
            if (previousTask != null && previousTask.Progress != task.Progress)
            {
                OnTaskProgressUpdated(new TaskStatusEventArgs(task, previousTask.Progress, task.Progress));

                if (task.IsCompleted && previousTask.Progress < task.RequiredProgress)
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

        OnLogMessageGenerated(new SimulationLogEventArgs($"Life Support: {Scenario.LifeSupport} (Decay: {decayDisplay}){lifeSupportTaskInfo}"));
        logWriter.WriteLine($"Life Support: {Scenario.LifeSupport} (Decay: {decayDisplay}){lifeSupportTaskInfo}");
        OnLogMessageGenerated(new SimulationLogEventArgs($"Colony Stats: {Scenario.ColonyStats.GetStatusSummary()}"));
        logWriter.WriteLine($"Colony Stats: {Scenario.ColonyStats.GetStatusSummary()}");
        OnLogMessageGenerated(new SimulationLogEventArgs(new string('=', 50)));
        logWriter.WriteLine(new string('=', 50));

        // Check if mission has failed after the update
        if (Scenario.HasFailed)
        {
            OnLogMessageGenerated(new SimulationLogEventArgs("💀 Mission failed! Life support has been depleted!"));
            logWriter.WriteLine("💀 Mission failed! Life support has been depleted!");
            CompleteSimulation();
            return false;
        }

        // Check if we've reached max steps
        if (CurrentStep >= MaxSteps)
        {
            OnLogMessageGenerated(new SimulationLogEventArgs("❌ Failed to resolve scenario within time limit."));
            logWriter.WriteLine("❌ Failed to resolve scenario within time limit.");
            CompleteSimulation();
            return false;
        }

        OnLogMessageGenerated(new SimulationLogEventArgs(""));
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

        OnLogMessageGenerated(new SimulationLogEventArgs("=== SIMULATION END ==="));
        logWriter.WriteLine("=== SIMULATION END ===");
        if (Scenario.IsSuccessful)
        {
            OnLogMessageGenerated(new SimulationLogEventArgs("✅ Scenario resolved successfully!"));
            logWriter.WriteLine("✅ Scenario resolved successfully!");
        }
        else if (Scenario.HasFailed)
        {
            OnLogMessageGenerated(new SimulationLogEventArgs("❌ Mission failed - Life support critical!"));
            logWriter.WriteLine("❌ Mission failed - Life support critical!");
        }
        else
        {
            OnLogMessageGenerated(new SimulationLogEventArgs("❌ Failed to resolve scenario within time limit."));
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
            OnLogMessageGenerated(new SimulationLogEventArgs("\nLife Support Maintenance Summary:"));
            logWriter.WriteLine("\nLife Support Maintenance Summary:");
            OnLogMessageGenerated(new SimulationLogEventArgs($"Maintenance Tasks Completed: {Scenario.CompletedLifeSupportTaskCount()}/{lifeSupportTasks.Count}"));
            logWriter.WriteLine($"Maintenance Tasks Completed: {Scenario.CompletedLifeSupportTaskCount()}/{lifeSupportTasks.Count}");
            OnLogMessageGenerated(new SimulationLogEventArgs($"Completion Rate: {Scenario.LifeSupportTaskCompletionRate() * 100:F1}%"));
            logWriter.WriteLine($"Completion Rate: {Scenario.LifeSupportTaskCompletionRate() * 100:F1}%");
            OnLogMessageGenerated(new SimulationLogEventArgs($"Final Life Support: {Scenario.LifeSupport}"));
            logWriter.WriteLine($"Final Life Support: {Scenario.LifeSupport}");
            OnLogMessageGenerated(new SimulationLogEventArgs($"Final Decay Rate: {Scenario.ActualLifeSupportDecay}/step (Base: {Scenario.LifeSupportDecay})"));
            logWriter.WriteLine($"Final Decay Rate: {Scenario.ActualLifeSupportDecay}/step (Base: {Scenario.LifeSupportDecay})");
        }

        // Show final task status
        OnLogMessageGenerated(new SimulationLogEventArgs("\nFinal Task Status:"));
        logWriter.WriteLine("\nFinal Task Status:");
        foreach (var task in Scenario.Tasks)
        {
            var status = task.IsCompleted ? "✅" : "❌";
            OnLogMessageGenerated(new SimulationLogEventArgs($"{status} {task.Name} ({task.Type}): {task.Progress}/{task.RequiredProgress}"));
            logWriter.WriteLine($"{status} {task.Name} ({task.Type}): {task.Progress}/{task.RequiredProgress}");
        }

        // Show detailed event summary
        if (Scenario.DetailedEventLog.Any())
        {
            OnLogMessageGenerated(new SimulationLogEventArgs("\nEvent Summary:"));
            logWriter.WriteLine("\nEvent Summary:");
            foreach (var eventEntry in Scenario.DetailedEventLog)
            {
                OnLogMessageGenerated(new SimulationLogEventArgs($"• {eventEntry.TimeStamp} - {eventEntry.EventName}"));
                logWriter.WriteLine($"• {eventEntry.TimeStamp} - {eventEntry.EventName}");
                OnLogMessageGenerated(new SimulationLogEventArgs($"  {eventEntry.EventDescription}"));
                logWriter.WriteLine($"  {eventEntry.EventDescription}");
                if (eventEntry.Effects.Any())
                {
                    OnLogMessageGenerated(new SimulationLogEventArgs("  Effects:"));
                    logWriter.WriteLine("  Effects:");
                    foreach (var effect in eventEntry.Effects)
                    {
                        OnLogMessageGenerated(new SimulationLogEventArgs($"    - {effect}"));
                        logWriter.WriteLine($"    - {effect}");
                    }
                }
                OnLogMessageGenerated(new SimulationLogEventArgs(""));
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
        return Scenario.Tasks
            .Select(task => new AgentSimulation.Events.TaskStatus
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Progress = task.Progress,
                RequiredProgress = task.RequiredProgress,
                IsCompleted = task.IsCompleted,
                Type = task.Type,
                IsImportant = task.IsImportant,
                ProgressPercentage = task.RequiredProgress > 0 ? (double)task.Progress / task.RequiredProgress * 100 : 0
            })
            .OrderBy(task => task.IsCompleted) // Completed tasks last
            .ThenByDescending(task => task.IsImportant) // Important tasks first
            .ThenByDescending(task => task.ProgressPercentage) // More progress first
            .ToList();
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
