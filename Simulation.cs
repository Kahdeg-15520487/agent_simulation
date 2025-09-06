using System;
using System.Collections.Generic;

namespace AgentSimulation;

public class Simulation
{
    public List<Agent> Agents { get; set; } = new();
    public Scenario Scenario { get; set; }

    public Simulation()
    {
        Scenario = new Scenario("Crashed Spaceship");
        Agents.Add(new Agent("Alice", "Brave"));
        Agents.Add(new Agent("Bob", "Cautious"));
        Agents.Add(new Agent("Charlie", "Logical"));
    }

    public void Run()
    {
        int steps = 0;
        while (!Scenario.IsResolved && steps < 20)
        {
            Console.WriteLine($"Step {steps + 1}:");
            foreach (var agent in Agents)
            {
                agent.Think(Scenario);
                agent.Act(Scenario);
            }
            Scenario.Update();
            Console.WriteLine($"Life Support: {Scenario.LifeSupport}");
            steps++;
        }
        if (Scenario.IsResolved)
            Console.WriteLine("Scenario resolved!");
        else
            Console.WriteLine("Failed to resolve.");
    }
}
