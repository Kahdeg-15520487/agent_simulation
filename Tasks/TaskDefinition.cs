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
    }

    public ActionType Type { get; set; }
    public int Value { get; set; }
    public string? NewTaskName { get; set; }
    public string? NewTaskDescription { get; set; }
    public int NewTaskRequiredProgress { get; set; } = 100;
    public TaskType NewTaskType { get; set; } = TaskType.Other;
    public bool IsRecurring { get; set; } = false; // For survival tasks that repeat
    public List<TaskCompletionAction>? NewTaskCompleteActions { get; set; } // Completion actions for the new task
    public string? EventMessage { get; set; } // Message for triggered events

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
    public List<TaskCompletionAction> CompletionActions { get; set; } = new();

    public TaskDefinition(string name, string description, int requiredProgress = 100, TaskType type = TaskType.Other)
    {
        Name = name;
        Description = description;
        RequiredProgress = requiredProgress;
        Type = type;
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
}
