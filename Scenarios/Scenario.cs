using System;
using System.Collections.Generic;
using System.Linq;
using AgentSimulation.Events;
using AgentSimulation.Tasks;
using AgentSimulation.Core;

namespace AgentSimulation.Scenarios;

public class Scenario
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<SimulationTask> Tasks { get; set; } = new();
    public int LifeSupport { get; set; }
    public int LifeSupportDecay { get; set; }
    public int ActualLifeSupportDecay { get; private set; } // The decay actually being applied
    public string? WinCondition { get; set; }
    public string? LoseCondition { get; set; }
    public SimulationTime Time { get; set; }
    public List<ActiveEvent> ActiveEvents { get; set; } = new();
    public List<EventLogEntry> DetailedEventLog { get; set; } = new();
    public List<string> EventLog { get; set; } = new(); // Keep for backward compatibility
    public int HoursPerStep { get; set; }

    public Scenario(ScenarioDefinition definition)
    {
        Name = definition.Name;
        Description = definition.Description;
        LifeSupport = definition.InitialLifeSupport;
        LifeSupportDecay = definition.LifeSupportDecay;
        ActualLifeSupportDecay = definition.LifeSupportDecay; // Initially same as base decay
        WinCondition = definition.WinCondition;
        LoseCondition = definition.LoseCondition;
        HoursPerStep = definition.HoursPerStep;
        Time = new SimulationTime();

        // Create tasks from definitions
        foreach (var taskDef in definition.TaskDefinitions)
        {
            Tasks.Add(new SimulationTask(taskDef.Name, taskDef.Description, taskDef.RequiredProgress, taskDef.Type));
        }

        // Initialize active events
        foreach (var eventDef in definition.EventDefinitions)
        {
            ActiveEvents.Add(new ActiveEvent(eventDef));
        }
    }

    public void Update()
    {
        // Advance time
        Time.AdvanceHours(HoursPerStep);

        // Process life support decay
        var lifeSupportTasks = Tasks.Where(t => t.Type == TaskType.Maintenance && t.IsCompleted).ToList();
        var actualDecay = LifeSupportDecay;
        
        // If any maintenance tasks are completed, reduce decay
        if (lifeSupportTasks.Any())
        {
            // Each completed maintenance task reduces decay
            var reductionFactor = Math.Max(0.1, 1.0 / (lifeSupportTasks.Count + 2)); // More tasks = better reduction
            actualDecay = Math.Max(1, (int)(LifeSupportDecay * reductionFactor));
            
            var taskNames = string.Join(", ", lifeSupportTasks.Select(t => t.Name));
            Console.WriteLine($"Maintenance systems operational ({taskNames})! Decay reduced to {actualDecay}/step");
        }
        
        ActualLifeSupportDecay = actualDecay; // Track the actual decay being applied
        LifeSupport -= actualDecay;
        if (LifeSupport <= 0) Console.WriteLine("Life support failed!");

        // Check for events
        CheckAndTriggerEvents();
    }

    private void CheckAndTriggerEvents()
    {
        var random = new Random();

        foreach (var activeEvent in ActiveEvents)
        {
            if (activeEvent.HasTriggered && activeEvent.Definition.IsOneTime)
                continue;

            bool shouldTrigger = false;

            switch (activeEvent.Definition.Trigger)
            {
                case EventTrigger.TimeBased:
                    if (Time.Hours >= activeEvent.Definition.TriggerHour)
                        shouldTrigger = true;
                    break;
                case EventTrigger.Random:
                    if (random.NextDouble() < activeEvent.Definition.TriggerProbability)
                        shouldTrigger = true;
                    break;
                case EventTrigger.Conditional:
                    // For now, treat as random
                    if (random.NextDouble() < activeEvent.Definition.TriggerProbability)
                        shouldTrigger = true;
                    break;
            }

            if (shouldTrigger)
            {
                TriggerEvent(activeEvent);
            }
        }
    }

    private void TriggerEvent(ActiveEvent activeEvent)
    {
        activeEvent.HasTriggered = true;

        var eventMsg = $"🚨 EVENT: {activeEvent.Definition.Name} - {activeEvent.Definition.Description}";
        Console.WriteLine(eventMsg);
        EventLog.Add(eventMsg);

        // Create detailed event log entry
        var eventLogEntry = new EventLogEntry(activeEvent.Definition.Name, activeEvent.Definition.Description, Time);
        
        foreach (var effect in activeEvent.Definition.Effects)
        {
            ApplyEventEffect(effect, eventLogEntry);
        }
        
        DetailedEventLog.Add(eventLogEntry);
    }

    private void ApplyEventEffect(EventEffect effect, EventLogEntry eventLogEntry)
    {
        switch (effect.Type)
        {
            case EventEffect.EffectType.ModifyLifeSupport:
                var oldLifeSupport = LifeSupport;
                LifeSupport += effect.Value;
                LifeSupport = Math.Max(0, Math.Min(200, LifeSupport)); // Clamp between 0-200
                var lifeSupportMsg = $"Life support {(effect.Value > 0 ? "increased" : "decreased")} by {Math.Abs(effect.Value)}";
                Console.WriteLine(lifeSupportMsg);
                eventLogEntry.Effects.Add($"Life Support: {oldLifeSupport} → {LifeSupport} ({(effect.Value > 0 ? "+" : "")}{effect.Value})");
                break;

            case EventEffect.EffectType.ModifyTaskProgress:
                var task = Tasks.FirstOrDefault(t => t.Name == effect.TaskName);
                if (task != null)
                {
                    var oldProgress = task.Progress;
                    task.UpdateProgress(effect.Value);
                    var taskMsg = $"Task '{task.Name}' progress {(effect.Value > 0 ? "increased" : "decreased")} by {Math.Abs(effect.Value)}";
                    Console.WriteLine(taskMsg);
                    eventLogEntry.Effects.Add($"Task '{task.Name}': {oldProgress}/{task.RequiredProgress} → {task.Progress}/{task.RequiredProgress} ({(effect.Value > 0 ? "+" : "")}{effect.Value})");
                }
                break;

            case EventEffect.EffectType.AddNewTask:
                if (!string.IsNullOrEmpty(effect.NewTaskName) && !string.IsNullOrEmpty(effect.NewTaskDescription))
                {
                    var newTask = new SimulationTask(effect.NewTaskName, effect.NewTaskDescription, effect.Value > 0 ? effect.Value : 100, TaskType.Other);
                    Tasks.Add(newTask);
                    var newTaskMsg = $"New task added: {effect.NewTaskName}";
                    Console.WriteLine(newTaskMsg);
                    eventLogEntry.Effects.Add($"New task added: '{effect.NewTaskName}' (Required: {newTask.RequiredProgress})");
                }
                break;

            case EventEffect.EffectType.ChangeLifeSupportDecay:
                var oldDecay = LifeSupportDecay;
                LifeSupportDecay += effect.Value;
                LifeSupportDecay = Math.Max(0, LifeSupportDecay); // Don't go below 0
                var decayMsg = $"Life support decay rate changed by {effect.Value}";
                Console.WriteLine(decayMsg);
                eventLogEntry.Effects.Add($"Life Support Decay: {oldDecay}/step → {LifeSupportDecay}/step ({(effect.Value > 0 ? "+" : "")}{effect.Value})");
                break;
        }
    }

    public bool IsResolved => Tasks.All(t => t.IsCompleted) && LifeSupport > 0;
    public bool HasFailed => LifeSupport <= 0;
    public bool IsSuccessful => Tasks.All(t => t.IsCompleted) && LifeSupport > 0;
    
    // Life support related task queries
    public List<SimulationTask> GetLifeSupportTasks() => Tasks.Where(t => t.Type == TaskType.Maintenance).ToList();
    public bool HasCompletedLifeSupportTasks() => Tasks.Any(t => t.Type == TaskType.Maintenance && t.IsCompleted);
    public int CompletedLifeSupportTaskCount() => Tasks.Count(t => t.Type == TaskType.Maintenance && t.IsCompleted);
    public double LifeSupportTaskCompletionRate() 
    {
        var maintenanceTasks = Tasks.Where(t => t.Type == TaskType.Maintenance).ToList();
        if (!maintenanceTasks.Any()) return 0.0;
        return (double)maintenanceTasks.Count(t => t.IsCompleted) / maintenanceTasks.Count;
    }
}
