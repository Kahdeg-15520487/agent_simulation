using System;
using AgentSimulation.Tasks;
using AgentSimulation.Agents;

namespace AgentSimulation.Events
{
    // Event argument classes for different simulation events
    public class SimulationStepEventArgs : EventArgs
    {
        public int StepNumber { get; }
        public int TotalSteps { get; }
        public double ProgressPercentage { get; }
        public DateTime Timestamp { get; }

        public SimulationStepEventArgs(int stepNumber, int totalSteps)
        {
            StepNumber = stepNumber;
            TotalSteps = totalSteps;
            ProgressPercentage = totalSteps > 0 ? (double)stepNumber / totalSteps * 100 : 0;
            Timestamp = DateTime.Now;
        }
    }

    public class TaskStatusEventArgs : EventArgs
    {
        public SimulationTask Task { get; }
        public int PreviousProgress { get; }
        public int NewProgress { get; }
        public bool JustCompleted { get; }
        public DateTime Timestamp { get; }

        public TaskStatusEventArgs(SimulationTask task, int previousProgress, int newProgress)
        {
            Task = task;
            PreviousProgress = previousProgress;
            NewProgress = newProgress;
            JustCompleted = !task.IsCompleted && newProgress >= task.RequiredProgress;
            Timestamp = DateTime.Now;
        }
    }

    public class TaskCompletedEventArgs : EventArgs
    {
        public SimulationTask Task { get; }
        public Agent? CompletedBy { get; }
        public DateTime Timestamp { get; }

        public TaskCompletedEventArgs(SimulationTask task, Agent? completedBy = null)
        {
            Task = task;
            CompletedBy = completedBy;
            Timestamp = DateTime.Now;
        }
    }

    public class AgentActionEventArgs : EventArgs
    {
        public Agent Agent { get; }
        public string Action { get; }
        public SimulationTask? TargetTask { get; }
        public DateTime Timestamp { get; }

        public AgentActionEventArgs(Agent agent, string action, SimulationTask? targetTask = null)
        {
            Agent = agent;
            Action = action;
            TargetTask = targetTask;
            Timestamp = DateTime.Now;
        }
    }

    public class SimulationStateEventArgs : EventArgs
    {
        public bool IsRunning { get; }
        public bool IsCompleted { get; }
        public bool IsPaused { get; }
        public string? StatusMessage { get; }
        public DateTime Timestamp { get; }

        public SimulationStateEventArgs(bool isRunning, bool isCompleted, bool isPaused, string? statusMessage = null)
        {
            IsRunning = isRunning;
            IsCompleted = isCompleted;
            IsPaused = isPaused;
            StatusMessage = statusMessage;
            Timestamp = DateTime.Now;
        }
    }

    public class UserInputRequestedEventArgs : EventArgs
    {
        public Agent RequestingAgent { get; }
        public string Prompt { get; }
        public List<string> Options { get; }
        public TaskCompletionSource<int> ResponseTask { get; }
        public DateTime Timestamp { get; }

        public UserInputRequestedEventArgs(Agent requestingAgent, string prompt, List<string> options)
        {
            RequestingAgent = requestingAgent;
            Prompt = prompt;
            Options = options;
            ResponseTask = new TaskCompletionSource<int>();
            Timestamp = DateTime.Now;
        }
    }

    public class SimulationLogEventArgs : EventArgs
    {
        public string Message { get; }
        public LogLevel Level { get; }
        public DateTime Timestamp { get; }

        public SimulationLogEventArgs(string message, LogLevel level = LogLevel.Info)
        {
            Message = message;
            Level = level;
            Timestamp = DateTime.Now;
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
