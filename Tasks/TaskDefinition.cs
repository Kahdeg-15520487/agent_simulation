namespace AgentSimulation.Tasks;

public enum TaskType
{
    Maintenance,     // Life support, equipment maintenance, repairs
    Resource,        // Gathering materials, supplies, information
    Engineering,     // Fixing machines, engines, weapons, building
    Combat,          // Attacking enemies, defending, tactical operations
    Research,        // Studying, analyzing, developing solutions
    Navigation,      // Movement, exploration, pathfinding
    Communication,   // Establishing contact, sending messages
    Medical,         // Healing, treating injuries, medical procedures
    Survival,        // Basic survival needs, shelter, food
    Other            // Miscellaneous tasks
}

public class TaskDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int RequiredProgress { get; set; } = 100;
    public TaskType Type { get; set; } = TaskType.Other;

    public TaskDefinition(string name, string description, int requiredProgress = 100, TaskType type = TaskType.Other)
    {
        Name = name;
        Description = description;
        RequiredProgress = requiredProgress;
        Type = type;
    }
}
