using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentSimulation;

public class Agent
{
    public string Name { get; set; }
    public string Personality { get; set; } // e.g., "Brave", "Cautious", "Logical"
    public List<string> Memory { get; set; } = new();
    public string CurrentThought { get; set; }

    public Agent(string name, string personality)
    {
        Name = name;
        Personality = personality;
        CurrentThought = "";
    }

    public virtual void Think(Scenario scenario)
    {
        // Generate thought based on personality and scenario
        CurrentThought = GenerateThought(scenario);
        Memory.Add(CurrentThought);
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

    public void Act(Scenario scenario)
    {
        // Choose a random incomplete task to work on
        var incompleteTasks = scenario.Tasks.Where(t => !t.IsCompleted).ToList();
        if (incompleteTasks.Any())
        {
            var random = new Random();
            var task = incompleteTasks[random.Next(incompleteTasks.Count)];
            var progressAmount = random.Next(5, 15);
            task.UpdateProgress(progressAmount);
            Console.WriteLine($"{Name} worked on {task.Name}: {CurrentThought}");
        }
    }
}
