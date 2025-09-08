using System;

namespace AgentSimulation.Core;

public class SimulationTime
{
    public int Hours { get; private set; }
    public int Days => Hours / 24;
    public int HoursInDay => Hours % 24;

    public SimulationTime(int startHours = 0)
    {
        Hours = startHours;
    }

    public void AdvanceHours(int hours)
    {
        Hours += hours;
    }

    public void AdvanceOneHour()
    {
        Hours += 1;
    }

    public override string ToString()
    {
        return $"Day {Days + 1}, {HoursInDay:00}:00";
    }
}
