namespace AgentSimulation.Tasks;

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
