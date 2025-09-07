# Agent Simulation

This is a C# console application that simulates agents in a scenario, such as a crashed spaceship on an alien planet. Agents have personalities, memory, and thoughts, and work together to resolve tasks while maintaining life support.

## Features

- Agents with different personalities: Brave, Cautious, Logical
- Memory system for agents
- Dynamic thoughts based on personality
- LLM-powered agents for more advanced thinking (supports local LLMs)
- Scenario with tasks to complete
- Life support system that deteriorates over time
- Simulation loop until resolution or failure

## LLM Agent Setup

The simulation includes an `LLMAgent` subclass that uses a local LLM for generating thoughts. By default, it connects to Ollama at `http://localhost:11434` with the `llama2` model.

### To use with local LLM:
1. Install and run [Ollama](https://ollama.ai/)
2. Pull a model: `ollama pull llama2`
3. Start Ollama: `ollama serve`
4. The simulation will automatically use the LLM for the Charlie agent

If the LLM is unavailable, it falls back to personality-based thoughts.

### Customization:
- Change the model in `Simulation.cs`: `new LLMAgent("Charlie", "Logical", "your-model")`
- Change the endpoint: `new LLMAgent("Charlie", "Logical", "llama2", "http://your-endpoint:port")`

## Time and Event System

The simulation now includes a dynamic time system and random/conditional events that make each run unique:

### Time System
- **Simulation Time**: Tracks hours and days as the simulation progresses
- **Configurable Pace**: Each scenario can have different hours per step
- **Time-Based Events**: Events can trigger at specific times

### Event System
Events add unpredictability and challenge to scenarios:

- **Event Types**: Positive (helpful), Negative (harmful), Neutral
- **Trigger Types**:
  - **Time-Based**: Trigger at specific simulation hours
  - **Random**: Probability-based chance each step
  - **Conditional**: Based on scenario state
- **Event Effects**:
  - Modify life support levels
  - Change task progress
  - Add new tasks
  - Alter life support decay rate

### Example Events
- **Alien Storm**: Reduces life support and increases decay rate
- **Resource Discovery**: Boosts resource gathering progress
- **Equipment Failure**: Time-based event that damages engine repairs
- **Zombie Horde**: Random attacks that threaten shelter integrity
- **Survivor Group**: Adds new tasks and improves life support

Events are displayed with 🚨 icons and logged for review at simulation end.

## How to Run

1. Ensure you have .NET 8.0 installed.
2. Navigate to the project directory.
3. Run `dotnet build` to compile.
4. Run `dotnet run` to start the simulation.

## Project Structure

- `Program.cs`: Entry point of the application.
- `Agent.cs`: Defines the base Agent class with personality, memory, and actions.
- `LLMAgent.cs`: Subclass of Agent that uses LLM for thinking.
- `Task.cs`: Defines the Task class for scenario objectives.
- `Scenario.cs`: Defines the Scenario class with tasks and life support.
- `ScenarioDefinition.cs`: Defines scenario templates and configurations.
- `ScenarioLibrary.cs`: Contains predefined scenario definitions.
- `SimulationTime.cs`: Handles simulation time tracking.
- `EventSystem.cs`: Defines events, effects, and event management.
- `Simulation.cs`: Manages the simulation loop and agents.
