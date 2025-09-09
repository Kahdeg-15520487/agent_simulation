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

    public override string Act(Scenario scenario, int _)
    {
        var logs = new StringBuilder();
        var incompleteTasks = scenario.Tasks.Where(t => !t.IsCompleted).ToList();
        if (!incompleteTasks.Any())
        {
            logs.AppendLine($"{Name}: All tasks are completed!");
            return logs.ToString();
        }

        int choice = GetUserChoiceAsync(incompleteTasks).Result;

        if (choice == -1)
        {
            logs.AppendLine($"{Name} decides to skip this turn.");
            return logs.ToString();
        }

        return base.Act(scenario, choice - 1);
    }

    private async Task<int> GetUserChoiceAsync(List<Tasks.SimulationTask> incompleteTasks)
    {
        if (this.Simulation == null) return 1;

        var options = new List<string>();
        for (int i = 0; i < incompleteTasks.Count; i++)
        {
            var task = incompleteTasks[i];
            var completionPercent = (task.Progress * 100.0 / task.RequiredProgress);
            var statusIcon = GetTaskStatusIcon(task);
            options.Add($"{statusIcon} {task.Name} [{task.Type}] - {completionPercent:F1}% complete");
        }
        options.Add("❌ Skip this turn");

        string prompt = $"Choose a task to work on:\n\n📊 Current situation:\n" +
                       $"� Life Support: {this.Simulation.GetSimulationStatus().LifeSupport}\n" +
                       $"⏱️ Step: {this.Simulation.GetSimulationStatus().CurrentStep}/{this.Simulation.GetSimulationStatus().MaxSteps}";

        int result = await this.Simulation.RequestUserInputAsync(this, prompt, options);

        // If user selected "Skip this turn" (last option), return -1
        if (result == options.Count - 1)
        {
            return -1;
        }

        // Otherwise return 1-based index for the selected task
        return result + 1;
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
