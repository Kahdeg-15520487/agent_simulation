using System.Collections.Generic;

namespace AgentSimulation;

public class ScenarioDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int InitialLifeSupport { get; set; } = 100;
    public int LifeSupportDecay { get; set; } = 5;
    public int HoursPerStep { get; set; } = 1; // How many hours pass per simulation step
    public List<EventDefinition> EventDefinitions { get; set; } = new();
    public List<TaskDefinition> TaskDefinitions { get; set; } = new();
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

public class TaskDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int RequiredProgress { get; set; } = 100;

    public TaskDefinition(string name, string description, int requiredProgress = 100)
    {
        Name = name;
        Description = description;
        RequiredProgress = requiredProgress;
    }
}
