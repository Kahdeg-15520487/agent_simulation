using System;
using System.Collections.Generic;

namespace AgentSimulation.Events;

public enum EventType
{
    Positive,
    Negative,
    Neutral
}

public enum EventTrigger
{
    TimeBased,
    Random,
    Conditional
}

public class EventDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public EventType Type { get; set; }
    public EventTrigger Trigger { get; set; }
    public int TriggerHour { get; set; } // For time-based events
    public double TriggerProbability { get; set; } // For random events (0.0 to 1.0)
    public List<EventEffect> Effects { get; set; } = new();
    public bool IsOneTime { get; set; } = true; // Can only happen once

    public EventDefinition(string name, string description, EventType type)
    {
        Name = name;
        Description = description;
        Type = type;
    }
}

public class EventEffect
{
    public enum EffectType
    {
        ModifyLifeSupport,
        ModifyTaskProgress,
        AddNewTask,
        RemoveTask,
        ChangeLifeSupportDecay
    }

    public EffectType Type { get; set; }
    public int Value { get; set; } // Amount to modify
    public string? TaskName { get; set; } // For task-related effects
    public string? NewTaskName { get; set; } // For adding new tasks
    public string? NewTaskDescription { get; set; }

    public EventEffect(EffectType type, int value = 0)
    {
        Type = type;
        Value = value;
    }
}

public class ActiveEvent
{
    public EventDefinition Definition { get; set; }
    public bool HasTriggered { get; set; }

    public ActiveEvent(EventDefinition definition)
    {
        Definition = definition;
        HasTriggered = false;
    }
}
