using System;
using System.Collections.Generic;
using System.Linq;
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
        Console.WriteLine("🏗️  TEAM CREATION");
        Console.WriteLine("================");
        Console.WriteLine("Choose how to create your team:");
        Console.WriteLine("1. 📋 Use a preset team");
        Console.WriteLine("2. 🎨 Create custom team");
        Console.Write("Enter choice (1-2): ");
        
        var choice = Console.ReadLine();
        if (choice == "1")
        {
            return CreatePresetTeam();
        }
        else
        {
            return CreateCustomTeam();
        }
    }

    private static List<Agent> CreatePresetTeam()
    {
        var presets = TeamPresets.GetAllPresets();
        var presetKeys = presets.Keys.ToArray();
        
        Console.WriteLine("\n📋 PRESET TEAMS");
        Console.WriteLine("===============");
        
        for (int i = 0; i < presetKeys.Length; i++)
        {
            var preset = presets[presetKeys[i]];
            Console.WriteLine($"{i + 1}. {preset.Name}");
            Console.WriteLine($"   {preset.Description}");
            
            // Show team composition
            Console.Write("   Team: ");
            for (int j = 0; j < preset.Agents.Count; j++)
            {
                var agent = preset.Agents[j];
                var typeIcon = agent.Type switch
                {
                    AgentType.BasicAI => "🤖",
                    AgentType.LLM => "🧠",
                    AgentType.Human => "🎮",
                    _ => "?"
                };
                Console.Write($"{agent.Name} {typeIcon}");
                if (j < preset.Agents.Count - 1) Console.Write(", ");
            }
            Console.WriteLine("\n");
        }
        
        while (true)
        {
            Console.Write($"Choose preset (1-{presetKeys.Length}): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= presetKeys.Length)
            {
                var selectedPreset = presets[presetKeys[choice - 1]];
                return BuildTeamFromPreset(selectedPreset);
            }
            Console.WriteLine("❌ Invalid choice. Please try again.");
        }
    }

    private static List<Agent> BuildTeamFromPreset(TeamPreset preset)
    {
        var team = new List<Agent>();
        
        Console.WriteLine($"\n✅ Building {preset.Name}...");
        
        foreach (var agentPreset in preset.Agents)
        {
            Agent agent = agentPreset.Type switch
            {
                AgentType.BasicAI => new Agent(agentPreset.Name, agentPreset.Personality),
                AgentType.LLM => new LLMAgent(agentPreset.Name, agentPreset.Personality, "http://localhost:8080"),
                AgentType.Human => new HumanAgent(agentPreset.Name),
                _ => new Agent(agentPreset.Name, agentPreset.Personality)
            };
            
            team.Add(agent);
            
            var typeIcon = agentPreset.Type switch
            {
                AgentType.BasicAI => "🤖",
                AgentType.LLM => "🧠",
                AgentType.Human => "🎮",
                _ => "?"
            };
            
            var agentTypeText = agentPreset.Type switch
            {
                AgentType.BasicAI => "Basic AI",
                AgentType.LLM => "AI Assistant",
                AgentType.Human => "Human Player",
                _ => "Unknown"
            };
            
            Console.WriteLine($"   ✅ {agent.Name} - {typeIcon} {agentTypeText} ({agent.Personality})");
        }
        
        Console.WriteLine($"\n� {preset.Name} ready for action!");
        Console.WriteLine();
        
        return team;
    }

    private static List<Agent> CreateCustomTeam()
    {
        var team = new List<Agent>();
        
        Console.WriteLine("\n🎨 CUSTOM TEAM CREATION");
        Console.WriteLine("=======================");
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
