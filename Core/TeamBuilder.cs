using System;
using System.Collections.Generic;
using AgentSimulation.Agents;

namespace AgentSimulation.Core;

public static class TeamBuilder
{
    private static readonly string[] DefaultNames = 
    {
        "Alex", "Maya", "Sam", "Jordan", "Riley", "Casey", "Morgan", "Taylor",
        "Jamie", "Quinn", "Avery", "Blake", "Cameron", "Drew", "Emery", "Finley"
    };
    
    public static List<Agent> CreateTeam()
    {
        var team = new List<Agent>();
        
        Console.WriteLine("🏗️  TEAM CREATION");
        Console.WriteLine("================");
        Console.WriteLine("Create your team of 3 agents. Choose from different agent types:");
        Console.WriteLine();
        
        for (int i = 1; i <= 3; i++)
        {
            Console.WriteLine($"👤 Agent #{i}:");
            var agent = CreateAgent(i);
            team.Add(agent);
            Console.WriteLine($"   ✅ {agent.Name} ({agent.Personality}) added to team!");
            Console.WriteLine();
        }
        
        Console.WriteLine("🎉 Team created successfully!");
        Console.WriteLine("Team members:");
        for (int i = 0; i < team.Count; i++)
        {
            var agent = team[i];
            var agentType = agent switch
            {
                HumanAgent => "Human Player",
                LLMAgent => "AI Assistant",
                _ => "Basic AI"
            };
            Console.WriteLine($"   {i + 1}. {agent.Name} - {agentType} ({agent.Personality})");
        }
        Console.WriteLine();
        
        return team;
    }
    
    private static string GetDefaultName(int agentNumber)
    {
        // Use modulo to cycle through names if we need more than available
        var index = (agentNumber - 1) % DefaultNames.Length;
        return DefaultNames[index];
    }
    
    private static Agent CreateAgent(int agentNumber)
    {
        while (true)
        {
            Console.WriteLine("Choose agent type:");
            Console.WriteLine("1. 🤖 Basic AI Agent");
            Console.WriteLine("2. 🧠 Advanced AI Agent (LLM)");
            Console.WriteLine("3. 🎮 Human Player");
            Console.Write("Enter choice (1-3): ");
            
            var input = Console.ReadLine();
            if (!int.TryParse(input, out int choice) || choice < 1 || choice > 3)
            {
                Console.WriteLine("❌ Invalid choice. Please enter 1, 2, or 3.");
                continue;
            }
            
            Console.Write($"Enter agent name (or press Enter for default '{GetDefaultName(agentNumber)}'): ");
            var name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                name = GetDefaultName(agentNumber);
                Console.WriteLine($"Using default name: {name}");
            }
            
            switch (choice)
            {
                case 1:
                    var personality = ChoosePersonality();
                    return new Agent(name, personality);
                    
                case 2:
                    var llmPersonality = ChoosePersonality();
                    Console.WriteLine("🧠 Advanced AI agent will use LLM for decision making.");
                    return new LLMAgent(name, llmPersonality, "http://localhost:8080");
                    
                case 3:
                    Console.WriteLine("🎮 You will control this agent's actions during the game.");
                    return new HumanAgent(name);
                    
                default:
                    continue;
            }
        }
    }
    
    private static string ChoosePersonality()
    {
        while (true)
        {
            Console.WriteLine("Choose personality:");
            Console.WriteLine("1. 💪 Brave - Acts quickly and decisively");
            Console.WriteLine("2. 🛡️  Cautious - Careful and risk-averse");
            Console.WriteLine("3. 🧮 Logical - Analytical and methodical");
            Console.Write("Enter choice (1-3): ");
            
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 3)
            {
                Console.WriteLine("❌ Invalid choice. Please enter 1, 2, or 3.");
                continue;
            }
            
            return choice switch
            {
                1 => "Brave",
                2 => "Cautious",
                3 => "Logical",
                _ => "Logical"
            };
        }
    }
}
