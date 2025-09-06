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
- `Simulation.cs`: Manages the simulation loop and agents.
