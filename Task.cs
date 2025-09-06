using System;
using System.Collections.Generic;

namespace AgentSimulation;

public class Task
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Progress { get; set; }
    public bool IsCompleted { get; set; }

    public Task(string name, string desc)
    {
        Name = name;
        Description = desc;
    }
}
