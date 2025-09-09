namespace AgentSimulation.Tasks;

public enum TaskType
{
    Maintenance,     // Life support, equipment maintenance, repairs
    Resource,        // Gathering materials, supplies, information
    Engineering,     // Fixing machines, engines, weapons, building
    Combat,          // Attacking enemies, defending, tactical operations
    Research,        // Studying, analyzing, developing solutions
    Navigation,      // Movement, exploration, pathfinding
    Communication,   // Establishing contact, sending messages
    Medical,         // Healing, treating injuries, medical procedures
    Survival,        // Basic survival needs, shelter, food
    Other            // Miscellaneous tasks
}

public class TaskCompletionAction
{
    public enum ActionType
    {
        AddNewTask,
        IncreaseLifeSupport,
        DecreaseLifeSupportDecay,
        UnlockNewTaskType,
        TriggerEvent,
        AddEffect,       // Add a buff or debuff to the scenario
    }

    public ActionType Type { get; set; }
    public int Value { get; set; }
    public string? NewTaskName { get; set; }
    public string? NewTaskDescription { get; set; }
    public int NewTaskRequiredProgress { get; set; } = 100;
    public TaskType NewTaskType { get; set; } = TaskType.Other;
    public bool NewTaskIsImportant { get; set; } = false; // Whether the new task should be marked as important
    public bool IsRecurring { get; set; } = false; // For survival tasks that repeat
    public List<TaskCompletionAction>? NewTaskCompleteActions { get; set; } // Completion actions for the new task
    public string? EventMessage { get; set; } // Message for triggered events
    
    // Effect-related properties
    public string? EffectName { get; set; }
    public string? EffectDescription { get; set; }
    public string? EffectType { get; set; } // "Buff" or "Debuff"
    public string? EffectTarget { get; set; } // "TaskType", "SpecificTask", "AllTasks", "LifeSupport", "LifeSupportDecay"
    public TaskType? EffectTargetTaskType { get; set; }
    public string? EffectTargetTaskName { get; set; }
    public double EffectMultiplier { get; set; } = 1.0;
    public int EffectFlatValue { get; set; } = 0;
    public int EffectDuration { get; set; } = -1; // -1 for permanent

    public TaskCompletionAction(ActionType type, int value = 0)
    {
        Type = type;
        Value = value;
    }

    public TaskCompletionAction(ActionType type, string eventMessage)
    {
        Type = type;
        EventMessage = eventMessage;
    }
}

public class TaskDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int RequiredProgress { get; set; } = 100;
    public TaskType Type { get; set; } = TaskType.Other;
    public bool IsImportant { get; set; } = false; // Important tasks appear first in ordering
    public List<TaskCompletionAction> CompletionActions { get; set; } = new();

    public TaskDefinition(string name, string description, int requiredProgress = 100, TaskType type = TaskType.Other, bool isImportant = false)
    {
        Name = name;
        Description = description;
        RequiredProgress = requiredProgress;
        Type = type;
        IsImportant = isImportant;
    }

    // Helper method to add completion actions
    public TaskDefinition AddCompletionAction(TaskCompletionAction action)
    {
        CompletionActions.Add(action);
        return this;
    }

    // Helper method for adding new task on completion
    public TaskDefinition AddsTaskOnCompletion(string taskName, string taskDescription, int requiredProgress = 100, TaskType taskType = TaskType.Other, bool isRecurring = false)
    {
        CompletionActions.Add(new TaskCompletionAction(TaskCompletionAction.ActionType.AddNewTask)
        {
            NewTaskName = taskName,
            NewTaskDescription = taskDescription,
            NewTaskRequiredProgress = requiredProgress,
            NewTaskType = taskType,
            IsRecurring = isRecurring
        });
        return this;
    }

    // Helper method for life support bonus on completion
    public TaskDefinition GivesLifeSupportOnCompletion(int lifeSupportBonus)
    {
        CompletionActions.Add(new TaskCompletionAction(TaskCompletionAction.ActionType.IncreaseLifeSupport, lifeSupportBonus));
        return this;
    }

    // Helper method to mark task as important
    public TaskDefinition MarkAsImportant()
    {
        IsImportant = true;
        return this;
    }

    // Helper method to add buff/debuff on completion
    public TaskDefinition AddsEffectOnCompletion(string effectName, string effectDescription, string effectType, string effectTarget, double multiplier = 1.0, int flatValue = 0, int duration = -1, TaskType? targetTaskType = null, string? targetTaskName = null)
    {
        CompletionActions.Add(new TaskCompletionAction(TaskCompletionAction.ActionType.AddEffect)
        {
            EffectName = effectName,
            EffectDescription = effectDescription,
            EffectType = effectType,
            EffectTarget = effectTarget,
            EffectMultiplier = multiplier,
            EffectFlatValue = flatValue,
            EffectDuration = duration,
            EffectTargetTaskType = targetTaskType,
            EffectTargetTaskName = targetTaskName
        });
        return this;
    }

    // Specific helper methods for common effect types
    public TaskDefinition AddsBuffToTaskType(string effectName, string effectDescription, TaskType taskType, double multiplier, int duration = -1)
    {
        return AddsEffectOnCompletion(effectName, effectDescription, "Buff", "TaskType", multiplier, 0, duration, taskType);
    }

    public TaskDefinition AddsDebuffToTaskType(string effectName, string effectDescription, TaskType taskType, double multiplier, int duration = -1)
    {
        return AddsEffectOnCompletion(effectName, effectDescription, "Debuff", "TaskType", multiplier, 0, duration, taskType);
    }

    public TaskDefinition AddsBuffToSpecificTask(string effectName, string effectDescription, string taskName, double multiplier, int duration = -1)
    {
        return AddsEffectOnCompletion(effectName, effectDescription, "Buff", "SpecificTask", multiplier, 0, duration, null, taskName);
    }

    public TaskDefinition AddsLifeSupportBuff(string effectName, string effectDescription, int flatValue, int duration = -1)
    {
        return AddsEffectOnCompletion(effectName, effectDescription, "Buff", "LifeSupport", 1.0, flatValue, duration);
    }

    public TaskDefinition AddsLifeSupportDecayReduction(string effectName, string effectDescription, double multiplier, int duration = -1)
    {
        return AddsEffectOnCompletion(effectName, effectDescription, "Buff", "LifeSupportDecay", multiplier, 0, duration);
    }
}
