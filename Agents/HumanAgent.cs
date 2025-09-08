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
        var incompleteTasks = scenario.Tasks.Where(t => !t.IsCompleted).ToList();
        if (!incompleteTasks.Any())
        {
            Console.WriteLine($"{Name}: All tasks are completed!");
            return;
        }

        Console.WriteLine($"\n🎯 {Name}'s Turn - Choose a task to work on:");
        Console.WriteLine($"Current Thought: {CurrentThought}");
        Console.WriteLine("\nAvailable Tasks:");
        
        for (int i = 0; i < incompleteTasks.Count; i++)
        {
            var task = incompleteTasks[i];
            var completionPercent = (task.Progress * 100.0 / task.RequiredProgress);
            var statusIcon = GetTaskStatusIcon(task);
            Console.WriteLine($"{i + 1}. {statusIcon} {task.Name} [{task.Type}]");
            Console.WriteLine($"   Progress: {task.Progress}/{task.RequiredProgress} ({completionPercent:F1}%)");
            Console.WriteLine($"   Description: {task.Description}");
        }

        // Show colony stats for informed decision making
        Console.WriteLine($"\n📊 Colony Stats: {scenario.ColonyStats.GetStatusSummary()}");
        Console.WriteLine($"💚 Life Support: {scenario.LifeSupport} (Efficiency: {scenario.ColonyStats.LifeSupportEfficiency:F1}x)");

        // Get user choice
        int choice = GetUserChoice(incompleteTasks.Count);
        if (choice == -1)
        {
            Console.WriteLine($"{Name} decides to skip this turn.");
            return;
        }

        var selectedTask = incompleteTasks[choice - 1];
        
        // Calculate progress with bonuses (using standard progress like other agents)
        var random = new Random();
        var baseProgress = random.Next(5, 15); // Same as other agents
        var bonusProgress = scenario.ColonyStats.CalculateTaskProgressBonus(selectedTask.Type);
        var totalProgress = baseProgress + bonusProgress;
        
        var oldProgress = selectedTask.Progress;
        selectedTask.UpdateProgress(totalProgress);
        
        // Report progress made
        Console.WriteLine($"\n✨ {Name} worked on {selectedTask.Name}: {CurrentThought}");
        if (bonusProgress > 0)
        {
            Console.WriteLine($"   Progress: {oldProgress} → {selectedTask.Progress}/{selectedTask.RequiredProgress} (+{baseProgress}+{bonusProgress} bonus)");
            Console.WriteLine($"   📈 Colony bonuses enhanced your work!");
        }
        else
        {
            Console.WriteLine($"   Progress: {oldProgress} → {selectedTask.Progress}/{selectedTask.RequiredProgress} (+{totalProgress})");
        }
        
        // Report if task was completed
        if (selectedTask.IsCompleted && oldProgress < selectedTask.RequiredProgress)
        {
            Console.WriteLine($"   🎉 {selectedTask.Name} COMPLETED!");
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
