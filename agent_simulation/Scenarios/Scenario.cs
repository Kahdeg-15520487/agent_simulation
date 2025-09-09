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
    public TextWriter LogsWriter { get; }
    public List<ActiveEvent> ActiveEvents { get; set; } = new();
    public List<EventLogEntry> DetailedEventLog { get; set; } = new();
    public List<string> EventLog { get; set; } = new(); // Keep for backward compatibility
    public int HoursPerStep { get; set; }
    public ColonyStats ColonyStats { get; set; } = new(); // Add colony stats
    public ScenarioDefinition Definition { get; private set; } // Store reference to definition
    private Random random;

    public Scenario(ScenarioDefinition definition,TextWriter logsWriter, int seed = -1)
    {
        Definition = definition; // Store the definition
        Name = definition.Name;
        Description = definition.Description;
        LifeSupport = definition.InitialLifeSupport;
        LifeSupportDecay = definition.LifeSupportDecay;
        ActualLifeSupportDecay = definition.LifeSupportDecay; // Initially same as base decay
        WinCondition = definition.WinCondition;
        LoseCondition = definition.LoseCondition;
        HoursPerStep = definition.HoursPerStep;
        Time = new SimulationTime();
        LogsWriter = logsWriter;

        // Create tasks from definitions
        foreach (var taskDef in definition.TaskDefinitions)
        {
            var task = new SimulationTask(taskDef);

            // Mark win condition tasks as important
            if (definition.WinConditionTasks.Contains(task.Name))
            {
                task.IsImportant = true;
            }

            if (!AddTaskSafely(task, allowDuplicates: true)) // Allow duplicates during initial load for flexibility
            {
                Console.WriteLine($"Warning: Duplicate task '{task.Name}' found in scenario definition");
            }
        }

        // Initialize active events
        foreach (var eventDef in definition.EventDefinitions)
        {
            ActiveEvents.Add(new ActiveEvent(eventDef));
        }
        if (seed == -1)
        {
            random = new Random();
        }
        else
        {
            random = new Random(seed);
        }
    }

    public void Update()
    {
        // Update colony stats first
        ColonyStats.UpdateStats(Tasks, LifeSupport);

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
            LogsWriter.WriteLine($"Maintenance systems operational ({taskNames})! Decay reduced to {actualDecay}/step");
        }

        ActualLifeSupportDecay = actualDecay; // Track the actual decay being applied
        LifeSupport -= actualDecay;
        if (LifeSupport <= 0) LogsWriter.WriteLine("Life support failed!");

        // Process task completion actions
        ProcessTaskCompletionActions();

        // Check for events
        CheckAndTriggerEvents();
    }

    private void CheckAndTriggerEvents()
    {
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
        LogsWriter.WriteLine(eventMsg);
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
                var actualDamage = effect.Value;

                // Apply defense mitigation for negative effects
                if (effect.Value < 0)
                {
                    actualDamage = -ColonyStats.CalculateEventDamageReduction(-effect.Value, eventLogEntry.EventName);
                    if (actualDamage != effect.Value)
                    {
                        LogsWriter.WriteLine($"🛡️ Colony defenses reduced damage from {-effect.Value} to {-actualDamage}!");
                    }
                }

                LifeSupport += actualDamage;
                LifeSupport = Math.Max(0, Math.Min(200, LifeSupport)); // Clamp between 0-200
                var lifeSupportMsg = $"Life support {(actualDamage > 0 ? "increased" : "decreased")} by {Math.Abs(actualDamage)}";
                LogsWriter.WriteLine(lifeSupportMsg);
                eventLogEntry.Effects.Add($"Life Support: {oldLifeSupport} → {LifeSupport} ({(actualDamage > 0 ? "+" : "")}{actualDamage})");
                break;

            case EventEffect.EffectType.ModifyTaskProgress:
                var task = Tasks.FirstOrDefault(t => t.Name == effect.TaskName);
                if (task != null)
                {
                    var oldProgress = task.Progress;
                    var actualChange = effect.Value;

                    // Apply defense mitigation for negative task effects on defense-related tasks
                    if (effect.Value < 0 && (task.Name.Contains("Fortify") || task.Type == TaskType.Combat))
                    {
                        var originalDamage = -effect.Value;
                        var mitigatedDamage = ColonyStats.CalculateEventDamageReduction(originalDamage, eventLogEntry.EventName);
                        actualChange = -mitigatedDamage;
                        if (actualChange != effect.Value)
                        {
                            LogsWriter.WriteLine($"🛡️ Existing defenses reduced task damage from {originalDamage} to {mitigatedDamage}!");
                        }
                    }

                    task.UpdateProgress(actualChange);
                    var taskMsg = $"Task '{task.Name}' progress {(actualChange > 0 ? "increased" : "decreased")} by {Math.Abs(actualChange)}";
                    LogsWriter.WriteLine(taskMsg);
                    eventLogEntry.Effects.Add($"Task '{task.Name}': {oldProgress}/{task.RequiredProgress} → {task.Progress}/{task.RequiredProgress} ({(actualChange > 0 ? "+" : "")}{actualChange})");
                }
                break;

            case EventEffect.EffectType.AddNewTask:
                if (!string.IsNullOrEmpty(effect.NewTaskName) && !string.IsNullOrEmpty(effect.NewTaskDescription))
                {
                    var newTask = new SimulationTask(effect.NewTaskName, effect.NewTaskDescription, effect.Value > 0 ? effect.Value : 100, TaskType.Other);
                    if (AddTaskSafely(newTask))
                    {
                        var newTaskMsg = $"New task added: {effect.NewTaskName}";
                        LogsWriter.WriteLine(newTaskMsg);
                        eventLogEntry.Effects.Add($"New task added: '{effect.NewTaskName}' (Required: {newTask.RequiredProgress})");
                    }
                    else
                    {
                        eventLogEntry.Effects.Add($"Duplicate task '{effect.NewTaskName}' prevented");
                    }
                }
                break;

            case EventEffect.EffectType.ChangeLifeSupportDecay:
                var oldDecay = LifeSupportDecay;
                LifeSupportDecay += effect.Value;
                LifeSupportDecay = Math.Max(0, LifeSupportDecay); // Don't go below 0
                var decayMsg = $"Life support decay rate changed by {effect.Value}";
                LogsWriter.WriteLine(decayMsg);
                eventLogEntry.Effects.Add($"Life Support Decay: {oldDecay}/step → {LifeSupportDecay}/step ({(effect.Value > 0 ? "+" : "")}{effect.Value})");
                break;
        }
    }

    public bool IsResolved => Tasks.All(t => t.IsCompleted) && LifeSupport > 0;
    public bool HasFailed => LifeSupport <= 0;
    public bool IsSuccessful => 
        LifeSupport > 0 && 
        (Definition.WinConditionTasks.Count == 0 || 
         Definition.WinConditionTasks.All(taskName => Tasks.Any(t => t.Name == taskName && t.IsCompleted)));

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

    private void ProcessTaskCompletionActions()
    {
        var tasksWithCompletionActions = Tasks.Where(t => t.ShouldTriggerCompletionActions()).ToList();
        
        foreach (var task in tasksWithCompletionActions)
        {
            LogsWriter.WriteLine($"🎯 Task '{task.Name}' completed - triggering effects...");
            
            foreach (var action in task.CompletionActions)
            {
                switch (action.Type)
                {
                    case TaskCompletionAction.ActionType.AddNewTask:
                        if (!string.IsNullOrEmpty(action.NewTaskName))
                        {
                            var newTaskDef = new TaskDefinition(
                                action.NewTaskName, 
                                action.NewTaskDescription ?? "Automatically generated task", 
                                action.NewTaskRequiredProgress, 
                                action.NewTaskType,
                                action.NewTaskIsImportant);
                            
                            // If it's a recurring task, add the same completion actions and inherit importance
                            if (action.IsRecurring)
                            {
                                newTaskDef.CompletionActions.AddRange(task.CompletionActions);
                                // Recurring tasks inherit the importance of their parent task
                                newTaskDef.IsImportant = task.IsImportant;
                            }
                            // If the action specifies completion actions for the new task, add them
                            else if (action.NewTaskCompleteActions != null && action.NewTaskCompleteActions.Count > 0)
                            {
                                newTaskDef.CompletionActions.AddRange(action.NewTaskCompleteActions);
                            }
                            
                            var newTask = new SimulationTask(newTaskDef);
                            // For recurring tasks, allow duplicates since they represent repeated actions
                            if (AddTaskSafely(newTask, action.IsRecurring))
                            {
                                LogsWriter.WriteLine($"   ➕ New task added: '{action.NewTaskName}' ({action.NewTaskType})");
                            }
                        }
                        break;
                        
                    case TaskCompletionAction.ActionType.IncreaseLifeSupport:
                        var oldLife = LifeSupport;
                        LifeSupport = Math.Min(200, LifeSupport + action.Value);
                        LogsWriter.WriteLine($"   💚 Life support increased: {oldLife} → {LifeSupport} (+{action.Value})");
                        break;
                        
                    case TaskCompletionAction.ActionType.DecreaseLifeSupportDecay:
                        var oldDecay = LifeSupportDecay;
                        LifeSupportDecay = Math.Max(0, LifeSupportDecay - action.Value);
                        LogsWriter.WriteLine($"   ⬇️ Life support decay reduced: {oldDecay} → {LifeSupportDecay} (-{action.Value})");
                        break;
                        
                    case TaskCompletionAction.ActionType.TriggerEvent:
                        if (!string.IsNullOrEmpty(action.EventMessage))
                        {
                            // Add the event message to the event log
                            EventLog.Add(action.EventMessage);
                            DetailedEventLog.Add(new EventLogEntry("Task Completion Event", action.EventMessage, Time));
                            LogsWriter.WriteLine($"   📢 Event triggered: {action.EventMessage}");
                        }
                        break;
                }
            }
            
            task.MarkCompletionActionsTriggered();
        }
    }

    /// <summary>
    /// Checks if a task with the given name already exists in the current task list
    /// </summary>
    /// <param name="taskName">The name of the task to check for</param>
    /// <returns>True if a task with the same name exists, false otherwise</returns>
    private bool TaskExists(string taskName)
    {
        return Tasks.Any(t => string.Equals(t.Name, taskName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Safely adds a new task to the task list, preventing duplicates
    /// </summary>
    /// <param name="newTask">The task to add</param>
    /// <param name="allowDuplicates">Whether to allow tasks with the same name (default: false)</param>
    /// <returns>True if the task was added, false if it was a duplicate and not added</returns>
    private bool AddTaskSafely(SimulationTask newTask, bool allowDuplicates = false)
    {
        if (!allowDuplicates && TaskExists(newTask.Name))
        {
            Console.WriteLine($"   ⚠️ Task '{newTask.Name}' already exists - skipping duplicate");
            return false;
        }
        
        Tasks.Add(newTask);
        return true;
    }
}
