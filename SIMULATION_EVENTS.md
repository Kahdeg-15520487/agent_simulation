# Simulation Event System Documentation

This document explains how to use the event system in the Agent Simulation project to create interactive user interfaces that can respond to simulation events.

## Overview

The simulation event system allows UI components to subscribe to various simulation events such as:
- Step completion
- Task progress updates 
- Task completion
- Agent actions
- Simulation state changes (start, pause, resume, stop, complete)
- User input requests
- Log messages

## Key Components

### 1. Event Arguments Classes (`SimulationEvents.cs`)

- **SimulationStepEventArgs**: Fired when simulation steps start/complete
- **TaskStatusEventArgs**: Fired when task progress is updated
- **TaskCompletedEventArgs**: Fired when a task is completed
- **AgentActionEventArgs**: Fired when an agent performs an action
- **SimulationStateEventArgs**: Fired for simulation state changes
- **UserInputRequestedEventArgs**: Fired when user input is needed
- **SimulationLogEventArgs**: Fired for log messages

### 2. Event Publisher Interface (`ISimulationEventPublisher.cs`)

The `ISimulationEventPublisher` interface defines all available events:

```csharp
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
```

### 3. Enhanced Simulation Class

The `Simulation` class now inherits from `SimulationEventPublisher` and fires events during simulation execution.

## How to Use the Event System

### Basic Event Subscription

```csharp
// Create simulation
var simulation = new Simulation(scenarioDefinition, agents, logWriter);

// Subscribe to events
simulation.TaskCompleted += (sender, e) =>
{
    Console.WriteLine($"Task completed: {e.Task.Name} by {e.CompletedBy?.Name}");
};

simulation.StepCompleted += (sender, e) =>
{
    Console.WriteLine($"Step {e.StepNumber}/{e.TotalSteps} completed ({e.ProgressPercentage:F0}%)");
};

simulation.SimulationCompleted += (sender, e) =>
{
    Console.WriteLine($"Simulation finished: {e.StatusMessage}");
};
```

### UI Integration Example

The `SimulationDashboard.cs` demonstrates a complete UI integration:

```csharp
private void SubscribeToSimulationEvents()
{
    if (simulation == null) return;

    // Update progress bars and labels
    simulation.StepStarted += OnStepStarted;
    simulation.StepCompleted += OnStepCompleted;
    
    // Update task list
    simulation.TaskProgressUpdated += OnTaskProgressUpdated;
    simulation.TaskCompleted += OnTaskCompleted;
    
    // Handle simulation state changes
    simulation.SimulationStarted += OnSimulationStarted;
    simulation.SimulationCompleted += OnSimulationCompleted;
    
    // Handle user input requests
    simulation.UserInputRequested += OnUserInputRequested;
}
```

### Handling User Input

For human agents that need user input, the event system provides a clean way to handle this:

```csharp
simulation.UserInputRequested += (sender, e) =>
{
    // Show UI for user input
    ShowUserInputDialog(e.Prompt, e.Options);
    
    // When user makes selection, complete the task
    int userChoice = GetUserSelection();
    e.ResponseTask.SetResult(userChoice);
};
```

### New Simulation Methods

The enhanced `Simulation` class provides additional methods for UI interaction:

```csharp
// Control simulation state
simulation.Pause();   // Pause the simulation
simulation.Resume();  // Resume the simulation
simulation.Stop();    // Stop the simulation

// Get current status for UI display
var taskStatuses = simulation.GetTaskStatuses();
var simStatus = simulation.GetSimulationStatus();

// Request user input asynchronously
int choice = await simulation.RequestUserInputAsync(agent, "Choose action:", options);
```

## Event Flow

1. **Simulation Start**: `SimulationStarted` event is fired
2. **Each Step**: 
   - `StepStarted` event fired
   - Agents perform actions (`AgentPerformedAction` events)
   - Tasks may update (`TaskProgressUpdated` events)
   - Tasks may complete (`TaskCompleted` events)
   - `StepCompleted` event fired
3. **User Input**: `UserInputRequested` event fired when human agents need input
4. **Simulation End**: `SimulationCompleted` or `SimulationStopped` event fired

## Features Demonstrated in SimulationDashboard

The `SimulationDashboard.cs` shows a complete implementation with:

- **Real-time Progress Tracking**: Progress bars for simulation and life support
- **Task Status Display**: ListView showing all tasks with progress and completion status
- **Interactive Controls**: Start, Stop, Pause, Step buttons
- **Auto-stepping**: Checkbox to enable automatic step progression
- **User Input Handling**: Modal dialog for human agent decisions
- **Live Logging**: Real-time display of simulation events and messages
- **Visual Feedback**: Color-coded status indicators and progress displays

## Benefits

1. **Decoupled Architecture**: UI components don't need direct access to simulation internals
2. **Real-time Updates**: UI automatically updates as simulation progresses
3. **Interactive Control**: Users can pause, resume, and step through simulations
4. **Extensible**: Easy to add new event types and UI responses
5. **Thread-safe**: Events can be safely handled across UI threads

This event system makes it easy to create rich, interactive user interfaces that provide real-time feedback and control over the simulation process.
