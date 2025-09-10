using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgentSimulation.Core;
using AgentSimulation.Scenarios;
using AgentSimulation.Tasks;

namespace AgentSimulation.Agents;

public class Agent
{
    public string Name { get; set; }
    public string Personality { get; set; } // e.g., "Brave", "Cautious", "Logical"
    public List<string> Memory { get; set; } = new();
    public string CurrentThought { get; set; }
    public Simulation? Simulation { get; set; }

    // Resource management properties
    public int Stamina { get; set; } = 100; // 0-100, affects work efficiency
    public int Food { get; set; } = 100; // 0-100, decreases over time
    public int Rest { get; set; } = 100; // 0-100, affects stamina recovery
    public bool IsResting { get; set; } = false; // Whether agent is currently resting
    public bool IsEating { get; set; } = false; // Whether agent is currently eating
    public int RestingStepsRemaining { get; set; } = 0; // Steps remaining for rest action
    public int EatingStepsRemaining { get; set; } = 0; // Steps remaining for eat action
    
    // Status thresholds
    public const int EXHAUSTED_THRESHOLD = 20;
    public const int HUNGRY_THRESHOLD = 30;
    public const int TIRED_THRESHOLD = 25;
    
    // Action durations (in simulation steps)
    public const int REST_DURATION = 3; // Rest takes 3 steps
    public const int EAT_DURATION = 2;  // Eating takes 2 steps

    public Agent(string name, string personality)
    {
        Name = name;
        Personality = personality;
        CurrentThought = "";
        Stamina = 100;
        Food = 100;
        Rest = 100;
        IsResting = false;
        IsEating = false;
        RestingStepsRemaining = 0;
        EatingStepsRemaining = 0;
    }

    public virtual string Think(Scenario scenario)
    {
        // Generate thought based on personality and scenario
        CurrentThought = GenerateThought(scenario);
        Memory.Add(CurrentThought);
        return CurrentThought;
    }

    protected string GenerateThought(Scenario scenario)
    {
        var random = new Random();
        List<string> thoughts = new();
        if (Personality == "Brave")
        {
            thoughts.Add("I must act quickly to save us!");
            thoughts.Add("Time is critical, let's push forward!");
            thoughts.Add("No time for hesitation, charge ahead!");
            if (scenario.LifeSupport < 50) thoughts.Add("Life support is failing, we need to hurry!");
        }
        else if (Personality == "Cautious")
        {
            thoughts.Add("I need to assess the risks carefully.");
            thoughts.Add("Let's proceed with caution.");
            thoughts.Add("Safety first, evaluate before acting.");
            if (scenario.LifeSupport < 50) thoughts.Add("The situation is dire, but we must be careful.");
        }
        else // Logical
        {
            thoughts.Add("Let's analyze the situation logically.");
            thoughts.Add("I need to think this through step by step.");
            thoughts.Add("Rational approach is key here.");
            if (scenario.LifeSupport < 50) thoughts.Add("Data shows life support critical, prioritize accordingly.");
        }
        return thoughts[random.Next(thoughts.Count)];
    }

    // GUID-based Act method for safer task referencing
    public virtual string Act(Scenario scenario, Guid? taskId = null)
    {
        var logs = new StringBuilder();
        
        // Handle ongoing rest/eat actions first
        if (IsResting && RestingStepsRemaining > 0)
        {
            RestingStepsRemaining--;
            TakeRest(); // Continue recovering
            logs.AppendLine($"{Name}: Continuing to rest... ({RestingStepsRemaining} steps remaining)");
            
            if (RestingStepsRemaining <= 0)
            {
                FinishRestingOrEating();
                logs.AppendLine($"{Name}: Finished resting and ready to work!");
            }
            return logs.ToString();
        }
        
        if (IsEating && EatingStepsRemaining > 0)
        {
            EatingStepsRemaining--;
            Eat(); // Continue recovering
            logs.AppendLine($"{Name}: Continuing to eat... ({EatingStepsRemaining} steps remaining)");
            
            if (EatingStepsRemaining <= 0)
            {
                FinishRestingOrEating();
                logs.AppendLine($"{Name}: Finished eating and ready to work!");
            }
            return logs.ToString();
        }
        
        // Check if agent needs to start rest/eat actions
        if (NeedsRest() && !IsResting && RestingStepsRemaining == 0)
        {
            IsResting = true;
            RestingStepsRemaining = REST_DURATION;
            TakeRest();
            logs.AppendLine($"{Name}: Starting to rest for {REST_DURATION} steps.");
            return logs.ToString();
        }

        if (NeedsFood() && !IsEating && EatingStepsRemaining == 0)
        {
            IsEating = true;
            EatingStepsRemaining = EAT_DURATION;
            Eat();
            logs.AppendLine($"{Name}: Starting to eat for {EAT_DURATION} steps.");
            return logs.ToString();
        }
        
        // Agent is available for tasks - consume resources for task work
        ConsumeResources();
        
        var incompleteTasks = scenario.Tasks.Where(t => !t.IsCompleted).ToList();
        if (incompleteTasks.Any())
        {
            var random = new Random();
            
            // Try to find task by GUID, fallback to random if not found
            SimulationTask task;
            if (taskId.HasValue)
            {
                task = incompleteTasks.FirstOrDefault(t => t.Id == taskId.Value) 
                       ?? incompleteTasks[random.Next(incompleteTasks.Count)];
            }
            else
            {
                task = incompleteTasks[random.Next(incompleteTasks.Count)];
            }

            var result = ActOnTask(scenario, task, new StringBuilder());
            return logs.ToString() + result;
        }
        
        logs.AppendLine($"{Name}: All tasks are completed!");
        return logs.ToString();
    }

    // Extracted common logic for working on a task
    private string ActOnTask(Scenario scenario, SimulationTask task, StringBuilder logs)
    {
        var random = new Random();
        
        // Base progress amount
        var baseProgress = random.Next(5, 15);

        // Apply colony stat bonuses
        var bonusProgress = scenario.ColonyStats.CalculateTaskProgressBonus(task.Type);
        
        // Apply effect multipliers and bonuses
        var effectMultiplier = scenario.EffectManager.GetTaskProgressMultiplier(task);
        var effectBonus = scenario.EffectManager.GetTaskProgressBonus(task);
        
        // Calculate final progress
        var totalBaseProgress = baseProgress + bonusProgress + effectBonus;
        var finalProgress = (int)Math.Round(totalBaseProgress * effectMultiplier);

        var oldProgress = task.Progress;
        task.UpdateProgress(finalProgress);

        // Report progress made
        logs.AppendLine($"{Name} worked on {task.Name}:");
        
        // Build detailed progress report
        var progressDetails = new List<string> { $"+{baseProgress} base" };
        if (bonusProgress > 0) progressDetails.Add($"+{bonusProgress} colony bonus");
        if (effectBonus != 0) progressDetails.Add($"{effectBonus:+0;-0} effect bonus");
        if (Math.Abs(effectMultiplier - 1.0) > 0.01) progressDetails.Add($"×{effectMultiplier:F1} effect multiplier");
        
        var detailsText = string.Join(", ", progressDetails);
        logs.AppendLine($"  Progress: {oldProgress} → {task.Progress}/{task.RequiredProgress} (+{finalProgress}: {detailsText})");

        // Report if task was completed
        if (task.IsCompleted && oldProgress < task.RequiredProgress)
        {
            logs.AppendLine($"  ✅ {task.Name} COMPLETED!");
        }

        return logs.ToString();
    }

    public virtual void ConsumeResources()
    {
        // Base consumption rates
        Stamina = Math.Max(0, Stamina - 5);  // Lose 5 stamina per action
        Food = Math.Max(0, Food - 3);        // Lose 3 food per action
        Rest = Math.Max(0, Rest - 2);        // Lose 2 rest per action
    }

    public virtual void TakeRest()
    {
        // Update thought if starting to rest
        if (IsResting && RestingStepsRemaining == REST_DURATION)
        {
            CurrentThought = "Taking a rest to recover stamina and rest...";
        }
        
        // Recover when resting - generous recovery since no consumption penalty
        Stamina = Math.Min(100, Stamina + 15);  // Good stamina recovery
        Rest = Math.Min(100, Rest + 20);        // Excellent rest recovery
        Food = Math.Max(0, Food - 1);           // Minimal food loss while resting
    }

    public virtual void Eat()
    {
        // Update thought if starting to eat
        if (IsEating && EatingStepsRemaining == EAT_DURATION)
        {
            CurrentThought = "Eating to restore food and energy...";
        }
        
        // Recover when eating - generous recovery since no consumption penalty  
        Food = Math.Min(100, Food + 20);        // Excellent food recovery
        Stamina = Math.Min(100, Stamina + 5);   // Small stamina boost from eating
        Rest = Math.Max(0, Rest - 1);           // Minimal rest loss while eating
    }

    public virtual bool NeedsRest()
    {
        return Stamina <= EXHAUSTED_THRESHOLD || Rest <= TIRED_THRESHOLD;
    }

    public virtual bool NeedsFood()
    {
        return Food <= HUNGRY_THRESHOLD;
    }

    public virtual bool CanWork()
    {
        // Agent cannot work if currently in the middle of rest/eat actions
        if (IsResting && RestingStepsRemaining > 0) return false;
        if (IsEating && EatingStepsRemaining > 0) return false;
        
        return Stamina > 0 && Food > 0 && Rest > 0;
    }

    public virtual void FinishRestingOrEating()
    {
        IsResting = false;
        IsEating = false;
        RestingStepsRemaining = 0;
        EatingStepsRemaining = 0;
    }
}
