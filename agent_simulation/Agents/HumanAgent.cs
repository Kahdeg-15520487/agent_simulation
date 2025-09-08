using System;
using System.Collections.Generic;
using System.Linq;
using AgentSimulation.Scenarios;

namespace AgentSimulation.Agents;

public class HumanAgent : Agent
{
    public HumanAgent(string name) : base(name, "Human-Controlled")
    {
    }

    public override void Think(Scenario scenario)
    {
        CurrentThought = "Analyzing the situation and considering options...";
        Memory.Add(CurrentThought);
    }

    public override void Act(Scenario scenario)
    {
        Logs.Clear();
        
        var incompleteTasks = scenario.Tasks.Where(t => !t.IsCompleted).ToList();
        if (!incompleteTasks.Any())
        {
            Logs.Add($"{Name}: All tasks are completed!");
            return;
        }

        Logs.Add($"\n🎯 {Name}'s Turn - Choose a task to work on:");
        Logs.Add($"Current Thought: {CurrentThought}");
        Logs.Add("\nAvailable Tasks:");
        
        for (int i = 0; i < incompleteTasks.Count; i++)
        {
            var task = incompleteTasks[i];
            var completionPercent = (task.Progress * 100.0 / task.RequiredProgress);
            var statusIcon = GetTaskStatusIcon(task);
            Logs.Add($"{i + 1}. {statusIcon} {task.Name} [{task.Type}]");
            Logs.Add($"   Progress: {task.Progress}/{task.RequiredProgress} ({completionPercent:F1}%)");
            Logs.Add($"   Description: {task.Description}");
        }

        // Show colony stats for informed decision making
        Logs.Add($"\n📊 Colony Stats: {scenario.ColonyStats.GetStatusSummary()}");
        Logs.Add($"💚 Life Support: {scenario.LifeSupport} (Efficiency: {scenario.ColonyStats.LifeSupportEfficiency:F1}x)");

        // For UI version, behave like a regular agent for now
        var random = new Random();
        var selectedTask = incompleteTasks[random.Next(incompleteTasks.Count)];
        
        // Calculate progress with bonuses
        var baseProgress = random.Next(5, 15);
        var bonusProgress = scenario.ColonyStats.CalculateTaskProgressBonus(selectedTask.Type);
        var totalProgress = baseProgress + bonusProgress;
        
        var oldProgress = selectedTask.Progress;
        selectedTask.UpdateProgress(totalProgress);
        
        // Report progress made
        Logs.Add($"{Name} worked on {selectedTask.Name}: {CurrentThought}");
        if (bonusProgress > 0)
        {
            Logs.Add($"  Progress: {oldProgress} → {selectedTask.Progress}/{selectedTask.RequiredProgress} (+{baseProgress}+{bonusProgress} bonus)");
        }
        else
        {
            Logs.Add($"  Progress: {oldProgress} → {selectedTask.Progress}/{selectedTask.RequiredProgress} (+{totalProgress})");
        }
        
        // Report if task was completed
        if (selectedTask.IsCompleted && oldProgress < selectedTask.RequiredProgress)
        {
            Logs.Add($"  🎉 {selectedTask.Name} COMPLETED!");
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

    private int GetUserChoice(int maxOptions)
    {
        while (true)
        {
            Console.Write($"\nEnter your choice (1-{maxOptions}, or 0 to skip): ");
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == 0)
                {
                    return -1; // Skip turn
                }
                if (choice >= 1 && choice <= maxOptions)
                {
                    return choice;
                }
            }
            Console.WriteLine("❌ Invalid choice. Please try again.");
        }
    }
}
