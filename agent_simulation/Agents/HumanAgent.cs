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

        // Use the GUID-based Act method with the chosen task
        return base.Act(scenario, chosenTask.Id);
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
        for (int i = 0; i < orderedTasks.Count; i++)
        {
            var task = orderedTasks[i];
            var completionPercent = (task.Progress * 100.0 / task.RequiredProgress);
            var statusIcon = GetTaskStatusIcon(task);
            var importantFlag = task.IsImportant ? "⭐ " : "";
            options.Add($"{statusIcon} {importantFlag}{task.Name} [{task.Type}] - {completionPercent:F1}% complete");
        }
        options.Add("❌ Skip this turn");

        string prompt = $"Choose a task to work on:\n\n📊 Current situation:\n" +
                       $"🫁 Life Support: {this.Simulation.GetSimulationStatus().LifeSupport}\n" +
                       $"⏱️ Step: {this.Simulation.GetSimulationStatus().CurrentStep}/{this.Simulation.GetSimulationStatus().MaxSteps}";

        int result = await this.Simulation.RequestUserInputAsync(this, prompt, options);

        // If user selected "Skip this turn" (last option), return null
        if (result == options.Count - 1)
        {
            return null;
        }

        // Return the selected task directly
        return orderedTasks[result];
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
