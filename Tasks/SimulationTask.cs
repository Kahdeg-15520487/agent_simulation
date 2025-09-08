using System;
using System.Collections.Generic;

namespace AgentSimulation.Tasks;

public class SimulationTask
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Progress { get; set; }
    public int RequiredProgress { get; set; }
    public bool IsCompleted { get; set; }
    public TaskType Type { get; set; }

    public SimulationTask(string name, string desc, int requiredProgress = 100, TaskType type = TaskType.Other)
    {
        Name = name;
        Description = desc;
        RequiredProgress = requiredProgress;
        Type = type;
    }

    public void UpdateProgress(int amount)
    {
        Progress += amount;
        if (Progress >= RequiredProgress)
        {
            IsCompleted = true;
        }
    }
}
