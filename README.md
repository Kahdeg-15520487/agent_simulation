# Agent Simulation

This is a C# console application that simulates agents in a scenario, such as a crashed spaceship on an alien planet. Agents have personalities, memory, and thoughts, and work together to resolve tasks while maintaining life support.

## Features

- Agents with different personalities: Brave, Cautious, Logical
- Memory system for agents
- Dynamic thoughts based on personality
- Scenario with tasks to complete
- Life support system that deteriorates over time
- Simulation loop until resolution or failure

## How to Run

1. Ensure you have .NET 8.0 installed.
2. Navigate to the project directory.
3. Run `dotnet build` to compile.
4. Run `dotnet run` to start the simulation.

## Project Structure

- `Program.cs`: Entry point of the application.
- `Agent.cs`: Defines the Agent class with personality, memory, and actions.
- `Task.cs`: Defines the Task class for scenario objectives.
- `Scenario.cs`: Defines the Scenario class with tasks and life support.
- `Simulation.cs`: Manages the simulation loop and agents.
