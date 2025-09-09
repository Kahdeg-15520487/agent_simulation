using AgentSimulation.Tasks;

namespace AgentSimulation.Events
{
    /// <summary>
    /// Represents the current status of a task for UI display
    /// </summary>
    public class TaskStatus
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Progress { get; set; }
        public int RequiredProgress { get; set; }
        public bool IsCompleted { get; set; }
        public TaskType Type { get; set; }
        public bool IsImportant { get; set; }
        public double ProgressPercentage { get; set; }
    }

    /// <summary>
    /// Represents the current status of the entire simulation for UI display
    /// </summary>
    public class SimulationStatus
    {
        public int CurrentStep { get; set; }
        public int MaxSteps { get; set; }
        public bool IsRunning { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPaused { get; set; }
        public int LifeSupport { get; set; }
        public int LifeSupportDecay { get; set; }
        public bool IsSuccessful { get; set; }
        public bool HasFailed { get; set; }
        public double ProgressPercentage { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
    }
}
