using System;
using System.Collections.Generic;

namespace AgentSimulation;

public class Task
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Progress { get; set; }
    public int RequiredProgress { get; set; }
    public bool IsCompleted { get; set; }

    public Task(string name, string desc, int requiredProgress = 100)
    {
        Name = name;
        Description = desc;
        RequiredProgress = requiredProgress;
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
