using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSimulation.Scenarios;
using AgentSimulation.Core;

namespace AgentSimulation.Agents;

public class HumanAgent : Agent
{
    public HumanAgent(string name) : base(name, "Human-Controlled") { }

    public override string Think(Scenario scenario)
    {
        CurrentThought = "Analyzing the situation and considering options...";
        Memory.Add(CurrentThought);
        return CurrentThought;
    }

    public override string Act(Scenario scenario, Guid? taskId = null)
    {
        var logs = new StringBuilder();
        
        // Handle ongoing rest/eat actions first (similar to base class)
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
        
        // Check if agent needs to rest or eat
        if (NeedsRest() && !IsResting)
        {
            logs.AppendLine($"{Name}: I'm getting tired/exhausted. Should I rest or continue working?");
        }
        
        if (NeedsFood() && !IsEating)
        {
            logs.AppendLine($"{Name}: I'm getting hungry. Should I eat or continue working?");
        }
        
        var incompleteTasks = scenario.Tasks.Where(t => !t.IsCompleted).ToList();
        if (!incompleteTasks.Any())
        {
            logs.AppendLine($"{Name}: All tasks are completed!");
            return logs.ToString();
        }

        var chosenTask = GetUserChoiceAsync(incompleteTasks).Result;

        if (chosenTask == null)
        {
            logs.AppendLine($"{Name} decides to skip this turn.");
            return logs.ToString();
        }

        // Handle special tasks (eat/rest)
        if (chosenTask.Id == Guid.Empty) // Rest task
        {
            if (!IsResting && RestingStepsRemaining == 0)
            {
                IsResting = true;
                RestingStepsRemaining = REST_DURATION;
                TakeRest();
                logs.AppendLine($"{Name}: Starting to rest for {REST_DURATION} steps.");
            }
            else
            {
                logs.AppendLine($"{Name}: Already resting or just finished resting.");
            }
            return logs.ToString();
        }
        else if (chosenTask.Id == new Guid("11111111-1111-1111-1111-111111111111")) // Eat task
        {
            if (!IsEating && EatingStepsRemaining == 0)
            {
                IsEating = true;
                EatingStepsRemaining = EAT_DURATION;
                Eat();
                logs.AppendLine($"{Name}: Starting to eat for {EAT_DURATION} steps.");
            }
            else
            {
                logs.AppendLine($"{Name}: Already eating or just finished eating.");
            }
            return logs.ToString();
        }

        // Use the GUID-based Act method with the chosen task
        var baseResult = base.Act(scenario, chosenTask.Id);
        return logs.ToString() + baseResult;
    }

    private async Task<Tasks.SimulationTask?> GetUserChoiceAsync(List<Tasks.SimulationTask> incompleteTasks)
    {
        if (this.Simulation == null) return null;

        // Order tasks: Important tasks first, then by progress (more progress first), completed tasks last
        var orderedTasks = incompleteTasks
            .OrderByDescending(task => task.IsImportant) // Important tasks first
            .ThenByDescending(task => (double)task.Progress / task.RequiredProgress) // More progress first
            .ToList();

        var options = new List<string>();
        
        // Add regular tasks
        for (int i = 0; i < orderedTasks.Count; i++)
        {
            var task = orderedTasks[i];
            var completionPercent = (task.Progress * 100.0 / task.RequiredProgress);
            var statusIcon = GetTaskStatusIcon(task);
            var importantFlag = task.IsImportant ? "⭐ " : "";
            options.Add($"{statusIcon} {importantFlag}{task.Name} [{task.Type}] - {completionPercent:F1}% complete");
        }
        
        // Add personal care options
        options.Add("🍽️ Take time to eat and restore energy");
        options.Add("😴 Take time to rest and recover stamina");
        options.Add("❌ Skip this turn");

        string prompt = $"Choose a task to work on:\n\n📊 Current situation:\n" +
                       $"🫁 Life Support: {this.Simulation.GetSimulationStatus().LifeSupport}\n" +
                       $"⏱️ Step: {this.Simulation.GetSimulationStatus().CurrentStep}/{this.Simulation.GetSimulationStatus().MaxSteps}\n" +
                       $"💪 Your Stamina: {Stamina}% | 🍽️ Food: {Food}% | 😴 Rest: {Rest}%";

        int result = await this.Simulation.RequestUserInputAsync(this, prompt, options);

        // Check what the user selected
        if (result == options.Count - 1) // Skip this turn (last option)
        {
            return null;
        }
        else if (result == options.Count - 2) // Rest option (second to last)
        {
            // Return a special "rest" task indicator
            return new Tasks.SimulationTask("Rest", "Taking time to rest and recover", 1, Tasks.TaskType.Other) { Id = Guid.Empty };
        }
        else if (result == options.Count - 3) // Eat option (third to last)
        {
            // Return a special "eat" task indicator  
            return new Tasks.SimulationTask("Eat", "Taking time to eat and restore energy", 1, Tasks.TaskType.Other) { Id = new Guid("11111111-1111-1111-1111-111111111111") };
        }
        else
        {
            // Return the selected regular task
            return orderedTasks[result];
        }
    }

    private string GetTaskStatusIcon(Tasks.SimulationTask task)
    {
        var completionPercent = (task.Progress * 100.0 / task.RequiredProgress);
        return completionPercent switch
        {
            >= 100 => "✅",
            >= 75 => "🔥",
            >= 50 => "⚡",
            >= 25 => "🔧",
            _ => "❗"
        };
    }
}
