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
    public string? WinCondition { get; set; }
    public string? LoseCondition { get; set; }
    public SimulationTime Time { get; set; }
    public List<ActiveEvent> ActiveEvents { get; set; } = new();
    public List<string> EventLog { get; set; } = new();
    public int HoursPerStep { get; set; }

    public Scenario(ScenarioDefinition definition)
    {
        Name = definition.Name;
        Description = definition.Description;
        LifeSupport = definition.InitialLifeSupport;
        LifeSupportDecay = definition.LifeSupportDecay;
        WinCondition = definition.WinCondition;
        LoseCondition = definition.LoseCondition;
        HoursPerStep = definition.HoursPerStep;
        Time = new SimulationTime();

        // Create tasks from definitions
        foreach (var taskDef in definition.TaskDefinitions)
        {
            Tasks.Add(new SimulationTask(taskDef.Name, taskDef.Description, taskDef.RequiredProgress));
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
        LifeSupport -= LifeSupportDecay;
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

        foreach (var effect in activeEvent.Definition.Effects)
        {
            ApplyEventEffect(effect);
        }
    }

    private void ApplyEventEffect(EventEffect effect)
    {
        switch (effect.Type)
        {
            case EventEffect.EffectType.ModifyLifeSupport:
                LifeSupport += effect.Value;
                LifeSupport = Math.Max(0, Math.Min(200, LifeSupport)); // Clamp between 0-200
                Console.WriteLine($"Life support {(effect.Value > 0 ? "increased" : "decreased")} by {Math.Abs(effect.Value)}");
                break;

            case EventEffect.EffectType.ModifyTaskProgress:
                var task = Tasks.FirstOrDefault(t => t.Name == effect.TaskName);
                if (task != null)
                {
                    task.UpdateProgress(effect.Value);
                    Console.WriteLine($"Task '{task.Name}' progress {(effect.Value > 0 ? "increased" : "decreased")} by {Math.Abs(effect.Value)}");
                }
                break;

            case EventEffect.EffectType.AddNewTask:
                if (!string.IsNullOrEmpty(effect.NewTaskName) && !string.IsNullOrEmpty(effect.NewTaskDescription))
                {
                    var newTask = new SimulationTask(effect.NewTaskName, effect.NewTaskDescription, effect.Value > 0 ? effect.Value : 100);
                    Tasks.Add(newTask);
                    Console.WriteLine($"New task added: {effect.NewTaskName}");
                }
                break;

            case EventEffect.EffectType.ChangeLifeSupportDecay:
                LifeSupportDecay += effect.Value;
                LifeSupportDecay = Math.Max(0, LifeSupportDecay); // Don't go below 0
                Console.WriteLine($"Life support decay rate changed by {effect.Value}");
                break;
        }
    }

    public bool IsResolved => Tasks.All(t => t.IsCompleted) && LifeSupport > 0;
}
