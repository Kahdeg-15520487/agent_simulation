using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgentSimulation.Scenarios;

namespace AgentSimulation.Agents;

public class HumanAgent : Agent
{
    private Func<int> actionSelector;

    public HumanAgent(string name, Func<int> actionSelector) : base(name, "Human-Controlled")
    {
        this.actionSelector = actionSelector;
    }

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

        //logs.AppendLine($"\n🎯 {Name}'s Turn - Choose a task to work on:");
        //logs.AppendLine($"Current Thought: {CurrentThought}");
        //logs.AppendLine("\nAvailable Tasks:");

        //for (int i = 0; i < incompleteTasks.Count; i++)
        //{
        //    var task = incompleteTasks[i];
        //    var completionPercent = (task.Progress * 100.0 / task.RequiredProgress);
        //    var statusIcon = GetTaskStatusIcon(task);
        //    logs.AppendLine($"{i + 1}. {statusIcon} {task.Name} [{task.Type}]");
        //    logs.AppendLine($"   Progress: {task.Progress}/{task.RequiredProgress} ({completionPercent:F1}%)");
        //    logs.AppendLine($"   Description: {task.Description}");
        //}

        // Show colony stats for informed decision making
        //logs.AppendLine($"\n📊 Colony Stats: {scenario.ColonyStats.GetStatusSummary()}");
        //logs.AppendLine($"💚 Life Support: {scenario.LifeSupport} (Efficiency: {scenario.ColonyStats.LifeSupportEfficiency:F1}x)");

        // Get user choice
        int choice = (actionSelector?.Invoke()).Value;
        if (choice == -1)
        {
            logs.AppendLine($"{Name} decides to skip this turn.");
            return logs.ToString();
        }

        return base.Act(scenario, choice - 1);
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
