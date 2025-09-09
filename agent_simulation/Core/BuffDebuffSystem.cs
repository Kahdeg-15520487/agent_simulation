using System;
using System.Collections.Generic;
using System.Linq;
using AgentSimulation.Tasks;

namespace AgentSimulation.Core;

public enum EffectType
{
    Buff,
    Debuff
}

public enum EffectTarget
{
    TaskType,        // Affects all tasks of a specific type (e.g., Engineering)
    SpecificTask,    // Affects a specific task by name
    AllTasks,        // Affects all tasks
    LifeSupport,     // Affects life support directly
    LifeSupportDecay // Affects life support decay rate
}

public class ScenarioEffect
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    public EffectType Type { get; set; }
    public EffectTarget Target { get; set; }
    public double Multiplier { get; set; } = 1.0;
    public int FlatValue { get; set; } = 0;
    public int Duration { get; set; } = -1; // -1 means permanent, positive numbers are steps remaining
    public int RemainingDuration { get; set; }
    public TaskType? TargetTaskType { get; set; }
    public string? TargetTaskName { get; set; }
    public string Source { get; set; } = "Unknown"; // What created this effect (task name, event, etc.)
    public bool IsActive { get; set; } = true;

    public ScenarioEffect(string name, string description, EffectType type, EffectTarget target, double multiplier = 1.0, int flatValue = 0, int duration = -1, string source = "Unknown")
    {
        Name = name;
        Description = description;
        Type = type;
        Target = target;
        Multiplier = multiplier;
        FlatValue = flatValue;
        Duration = duration;
        RemainingDuration = duration;
        Source = source;
    }

    /// <summary>
    /// Create a task-type specific effect
    /// </summary>
    public static ScenarioEffect ForTaskType(string name, string description, EffectType type, TaskType taskType, double multiplier, int flatValue = 0, int duration = -1, string source = "Unknown")
    {
        return new ScenarioEffect(name, description, type, EffectTarget.TaskType, multiplier, flatValue, duration, source)
        {
            TargetTaskType = taskType
        };
    }

    /// <summary>
    /// Create a specific task effect
    /// </summary>
    public static ScenarioEffect ForSpecificTask(string name, string description, EffectType type, string taskName, double multiplier, int flatValue = 0, int duration = -1, string source = "Unknown")
    {
        return new ScenarioEffect(name, description, type, EffectTarget.SpecificTask, multiplier, flatValue, duration, source)
        {
            TargetTaskName = taskName
        };
    }

    /// <summary>
    /// Create a life support effect
    /// </summary>
    public static ScenarioEffect ForLifeSupport(string name, string description, EffectType type, int flatValue, int duration = -1, string source = "Unknown")
    {
        return new ScenarioEffect(name, description, type, EffectTarget.LifeSupport, 1.0, flatValue, duration, source);
    }

    /// <summary>
    /// Create a life support decay effect
    /// </summary>
    public static ScenarioEffect ForLifeSupportDecay(string name, string description, EffectType type, double multiplier, int duration = -1, string source = "Unknown")
    {
        return new ScenarioEffect(name, description, type, EffectTarget.LifeSupportDecay, multiplier, 0, duration, source);
    }

    /// <summary>
    /// Check if this effect applies to a specific task
    /// </summary>
    public bool AppliesTo(SimulationTask task)
    {
        if (!IsActive) return false;

        return Target switch
        {
            EffectTarget.AllTasks => true,
            EffectTarget.TaskType => TargetTaskType == task.Type,
            EffectTarget.SpecificTask => TargetTaskName?.Equals(task.Name, StringComparison.OrdinalIgnoreCase) == true,
            _ => false
        };
    }

    /// <summary>
    /// Update the effect for one time step
    /// </summary>
    public void UpdateDuration()
    {
        if (Duration > 0)
        {
            RemainingDuration--;
            if (RemainingDuration <= 0)
            {
                IsActive = false;
            }
        }
    }

    /// <summary>
    /// Get a display string for the effect
    /// </summary>
    public string GetDisplayString()
    {
        var typeIcon = Type == EffectType.Buff ? "↗️" : "↘️";
        var targetDesc = Target switch
        {
            EffectTarget.AllTasks => "All Tasks",
            EffectTarget.TaskType => $"{TargetTaskType} Tasks",
            EffectTarget.SpecificTask => $"Task: {TargetTaskName}",
            EffectTarget.LifeSupport => "Life Support",
            EffectTarget.LifeSupportDecay => "Life Support Decay",
            _ => "Unknown"
        };

        var durationText = Duration == -1 ? "Permanent" : $"{RemainingDuration} steps left";
        var effectText = Multiplier != 1.0 ? $"×{Multiplier:F1}" : (FlatValue != 0 ? $"{FlatValue:+0;-0}" : "");

        return $"{typeIcon} {Name} ({targetDesc}) {effectText} - {durationText}";
    }
}

public class EffectManager
{
    private List<ScenarioEffect> activeEffects = new();

    public IReadOnlyList<ScenarioEffect> ActiveEffects => activeEffects.AsReadOnly();

    /// <summary>
    /// Add a new effect to the scenario
    /// </summary>
    public void AddEffect(ScenarioEffect effect)
    {
        // Check if this is a duplicate effect
        var existingEffect = activeEffects.FirstOrDefault(e => 
            e.Name == effect.Name && 
            e.Target == effect.Target && 
            e.TargetTaskType == effect.TargetTaskType &&
            e.TargetTaskName == effect.TargetTaskName);

        if (existingEffect != null)
        {
            // Refresh duration or stack the effect
            if (effect.Duration > existingEffect.RemainingDuration)
            {
                existingEffect.RemainingDuration = effect.Duration;
            }
            // Optionally stack multipliers or flat values here
        }
        else
        {
            activeEffects.Add(effect);
        }
    }

    /// <summary>
    /// Remove an effect by ID
    /// </summary>
    public bool RemoveEffect(Guid effectId)
    {
        var effect = activeEffects.FirstOrDefault(e => e.Id == effectId);
        if (effect != null)
        {
            return activeEffects.Remove(effect);
        }
        return false;
    }

    /// <summary>
    /// Update all effects for one time step
    /// </summary>
    public List<ScenarioEffect> UpdateEffects()
    {
        var expiredEffects = new List<ScenarioEffect>();

        foreach (var effect in activeEffects)
        {
            effect.UpdateDuration();
            if (!effect.IsActive)
            {
                expiredEffects.Add(effect);
            }
        }

        // Remove expired effects
        foreach (var expired in expiredEffects)
        {
            activeEffects.Remove(expired);
        }

        return expiredEffects;
    }

    /// <summary>
    /// Calculate the effective progress multiplier for a task considering all applicable effects
    /// </summary>
    public double GetTaskProgressMultiplier(SimulationTask task)
    {
        double multiplier = 1.0;

        foreach (var effect in activeEffects.Where(e => e.AppliesTo(task) && e.IsActive))
        {
            multiplier *= effect.Multiplier;
        }

        return multiplier;
    }

    /// <summary>
    /// Calculate the flat progress bonus for a task considering all applicable effects
    /// </summary>
    public int GetTaskProgressBonus(SimulationTask task)
    {
        int bonus = 0;

        foreach (var effect in activeEffects.Where(e => e.AppliesTo(task) && e.IsActive))
        {
            bonus += effect.FlatValue;
        }

        return bonus;
    }

    /// <summary>
    /// Get life support effects
    /// </summary>
    public int GetLifeSupportBonus()
    {
        return activeEffects
            .Where(e => e.Target == EffectTarget.LifeSupport && e.IsActive)
            .Sum(e => e.FlatValue);
    }

    /// <summary>
    /// Get life support decay multiplier
    /// </summary>
    public double GetLifeSupportDecayMultiplier()
    {
        double multiplier = 1.0;

        foreach (var effect in activeEffects.Where(e => e.Target == EffectTarget.LifeSupportDecay && e.IsActive))
        {
            multiplier *= effect.Multiplier;
        }

        return multiplier;
    }

    /// <summary>
    /// Get all effects as display strings
    /// </summary>
    public List<string> GetEffectDisplayStrings()
    {
        return activeEffects
            .Where(e => e.IsActive)
            .OrderBy(e => e.Type)
            .ThenBy(e => e.Target)
            .Select(e => e.GetDisplayString())
            .ToList();
    }

    /// <summary>
    /// Clear all effects
    /// </summary>
    public void ClearAllEffects()
    {
        activeEffects.Clear();
    }
}
