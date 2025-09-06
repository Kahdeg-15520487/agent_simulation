using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentSimulation;

public class Scenario
{
    public string Name { get; set; }
    public List<Task> Tasks { get; set; } = new();
    public int LifeSupport { get; set; } = 100;

    public Scenario(string name)
    {
        Name = name;
        // Add tasks
        Tasks.Add(new Task("Fix Engine", "Repair the spaceship engine."));
        Tasks.Add(new Task("Gather Resources", "Collect materials from the planet."));
        Tasks.Add(new Task("Maintain Life Support", "Ensure oxygen and power."));
    }

    public void Update()
    {
        LifeSupport -= 5; // deteriorate
        if (LifeSupport <= 0) Console.WriteLine("Life support failed!");
    }

    public bool IsResolved => Tasks.All(t => t.IsCompleted) && LifeSupport > 0;
}
