using System;
using System.Collections.Generic;

namespace AgentSimulation.Tasks;

public class SimulationTask
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Unique identifier for safe task referencing
    public string Name { get; set; }
    public string Description { get; set; }
    public int Progress { get; set; }
    public int RequiredProgress { get; set; }
    public bool IsCompleted { get; set; }
    public TaskType Type { get; set; }
    public bool IsImportant { get; set; } = false; // Important tasks appear first in ordering
    public List<TaskCompletionAction> CompletionActions { get; set; } = new();
    public bool HasTriggeredCompletion { get; set; } = false; // Prevent multiple triggers

    public SimulationTask(string name, string desc, int requiredProgress = 100, TaskType type = TaskType.Other, bool isImportant = false)
    {
        Name = name;
        Description = desc;
        RequiredProgress = requiredProgress;
        Type = type;
        IsImportant = isImportant;
    }

    // Constructor that accepts completion actions
    public SimulationTask(TaskDefinition definition)
    {
        Name = definition.Name;
        Description = definition.Description;
        RequiredProgress = definition.RequiredProgress;
        Type = definition.Type;
        IsImportant = definition.IsImportant;
        CompletionActions = new List<TaskCompletionAction>(definition.CompletionActions);
    }

    public void UpdateProgress(int amount)
    {
        Progress += amount;
        if (Progress >= RequiredProgress)
        {
            IsCompleted = true;
        }
    }

    // Check if this task should trigger completion actions
    public bool ShouldTriggerCompletionActions()
    {
        return IsCompleted && !HasTriggeredCompletion && CompletionActions.Count > 0;
    }

    // Mark completion actions as triggered
    public void MarkCompletionActionsTriggered()
    {
        HasTriggeredCompletion = true;
    }
}
