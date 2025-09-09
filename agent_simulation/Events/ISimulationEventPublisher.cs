using System;

namespace AgentSimulation.Events
{
    /// <summary>
    /// Interface for objects that can publish simulation events
    /// </summary>
    public interface ISimulationEventPublisher
    {
        // Step-related events
        event EventHandler<SimulationStepEventArgs>? StepStarted;
        event EventHandler<SimulationStepEventArgs>? StepCompleted;

        // Task-related events
        event EventHandler<TaskStatusEventArgs>? TaskProgressUpdated;
        event EventHandler<TaskCompletedEventArgs>? TaskCompleted;

        // Agent-related events
        event EventHandler<AgentActionEventArgs>? AgentPerformedAction;

        // Simulation state events
        event EventHandler<SimulationStateEventArgs>? SimulationStarted;
        event EventHandler<SimulationStateEventArgs>? SimulationPaused;
        event EventHandler<SimulationStateEventArgs>? SimulationResumed;
        event EventHandler<SimulationStateEventArgs>? SimulationCompleted;
        event EventHandler<SimulationStateEventArgs>? SimulationStopped;

        // User interaction events
        event EventHandler<UserInputRequestedEventArgs>? UserInputRequested;

        // Logging events
        event EventHandler<SimulationLogEventArgs>? LogMessageGenerated;
    }

    /// <summary>
    /// Base implementation of simulation event publisher with helper methods
    /// </summary>
    public abstract class SimulationEventPublisher : ISimulationEventPublisher
    {
        // Step-related events
        public event EventHandler<SimulationStepEventArgs>? StepStarted;
        public event EventHandler<SimulationStepEventArgs>? StepCompleted;

        // Task-related events
        public event EventHandler<TaskStatusEventArgs>? TaskProgressUpdated;
        public event EventHandler<TaskCompletedEventArgs>? TaskCompleted;

        // Agent-related events
        public event EventHandler<AgentActionEventArgs>? AgentPerformedAction;

        // Simulation state events
        public event EventHandler<SimulationStateEventArgs>? SimulationStarted;
        public event EventHandler<SimulationStateEventArgs>? SimulationPaused;
        public event EventHandler<SimulationStateEventArgs>? SimulationResumed;
        public event EventHandler<SimulationStateEventArgs>? SimulationCompleted;
        public event EventHandler<SimulationStateEventArgs>? SimulationStopped;

        // User interaction events
        public event EventHandler<UserInputRequestedEventArgs>? UserInputRequested;

        // Logging events
        public event EventHandler<SimulationLogEventArgs>? LogMessageGenerated;

        // Helper methods to safely invoke events
        protected virtual void OnStepStarted(SimulationStepEventArgs e)
        {
            StepStarted?.Invoke(this, e);
        }

        protected virtual void OnStepCompleted(SimulationStepEventArgs e)
        {
            StepCompleted?.Invoke(this, e);
        }

        protected virtual void OnTaskProgressUpdated(TaskStatusEventArgs e)
        {
            TaskProgressUpdated?.Invoke(this, e);
        }

        protected virtual void OnTaskCompleted(TaskCompletedEventArgs e)
        {
            TaskCompleted?.Invoke(this, e);
        }

        protected virtual void OnAgentPerformedAction(AgentActionEventArgs e)
        {
            AgentPerformedAction?.Invoke(this, e);
        }

        protected virtual void OnSimulationStarted(SimulationStateEventArgs e)
        {
            SimulationStarted?.Invoke(this, e);
        }

        protected virtual void OnSimulationPaused(SimulationStateEventArgs e)
        {
            SimulationPaused?.Invoke(this, e);
        }

        protected virtual void OnSimulationResumed(SimulationStateEventArgs e)
        {
            SimulationResumed?.Invoke(this, e);
        }

        protected virtual void OnSimulationCompleted(SimulationStateEventArgs e)
        {
            SimulationCompleted?.Invoke(this, e);
        }

        protected virtual void OnSimulationStopped(SimulationStateEventArgs e)
        {
            SimulationStopped?.Invoke(this, e);
        }

        protected virtual void OnUserInputRequested(UserInputRequestedEventArgs e)
        {
            UserInputRequested?.Invoke(this, e);
        }

        protected virtual void OnLogMessageGenerated(SimulationLogEventArgs e)
        {
            LogMessageGenerated?.Invoke(this, e);
        }
    }
}
