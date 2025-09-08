using System.Collections.Generic;
using AgentSimulation.Events;
using AgentSimulation.Tasks;

namespace AgentSimulation.Scenarios;

public class ScenarioDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int InitialLifeSupport { get; set; } = 100;
    public int LifeSupportDecay { get; set; } = 5;
    public int HoursPerStep { get; set; } = 1; // How many hours pass per simulation step
    public List<EventDefinition> EventDefinitions { get; set; } = new();
    public List<TaskDefinition> TaskDefinitions { get; set; } = new();
    public List<string> WinConditionTasks { get; set; } = new(); // Tasks that must be completed to win
    public string? WinCondition { get; set; } // Description of how to win
    public string? LoseCondition { get; set; } // Description of how to lose

    public ScenarioDefinition(string name, string description)
    {
        Name = name;
        Description = description;
        WinCondition = "Complete all tasks before life support fails";
        LoseCondition = "Life support reaches 0";
    }
}
