using System.Collections.Generic;
using AgentSimulation.Agents;

namespace AgentSimulation.Core;

public static class TeamPresets
{
    public static Dictionary<string, TeamPreset> GetAllPresets()
    {
        return new Dictionary<string, TeamPreset>
        {
            ["balanced"] = new TeamPreset
            {
                Name = "🏗️ Balanced Team",
                Description = "A well-rounded team with diverse personalities and skills",
                Agents = new List<AgentPreset>
                {
                    new AgentPreset { Name = "Alex", Type = AgentType.BasicAI, Personality = "Brave" },
                    new AgentPreset { Name = "Maya", Type = AgentType.LLM, Personality = "Logical" },
                    new AgentPreset { Name = "Sam", Type = AgentType.Human, Personality = "Human-Controlled" }
                }
            },
            ["ai_squad"] = new TeamPreset
            {
                Name = "🤖 AI Squad",
                Description = "Three AI agents with different decision-making approaches",
                Agents = new List<AgentPreset>
                {
                    new AgentPreset { Name = "Commander", Type = AgentType.BasicAI, Personality = "Brave" },
                    new AgentPreset { Name = "Analyst", Type = AgentType.LLM, Personality = "Logical" },
                    new AgentPreset { Name = "Guardian", Type = AgentType.BasicAI, Personality = "Cautious" }
                }
            },
            ["human_leader"] = new TeamPreset
            {
                Name = "👑 Human Leader",
                Description = "You lead two AI assistants with complementary personalities",
                Agents = new List<AgentPreset>
                {
                    new AgentPreset { Name = "Captain", Type = AgentType.Human, Personality = "Human-Controlled" },
                    new AgentPreset { Name = "Advisor", Type = AgentType.LLM, Personality = "Logical" },
                    new AgentPreset { Name = "Scout", Type = AgentType.BasicAI, Personality = "Brave" }
                }
            },
            ["research_team"] = new TeamPreset
            {
                Name = "🧪 Research Team",
                Description = "Logical thinkers focused on analysis and careful planning",
                Agents = new List<AgentPreset>
                {
                    new AgentPreset { Name = "Dr. Chen", Type = AgentType.LLM, Personality = "Logical" },
                    new AgentPreset { Name = "Dr. Patel", Type = AgentType.BasicAI, Personality = "Logical" },
                    new AgentPreset { Name = "Lab Assistant", Type = AgentType.BasicAI, Personality = "Cautious" }
                }
            },
            ["action_heroes"] = new TeamPreset
            {
                Name = "💪 Action Heroes",
                Description = "Bold and decisive agents who act first and ask questions later",
                Agents = new List<AgentPreset>
                {
                    new AgentPreset { Name = "Blaze", Type = AgentType.BasicAI, Personality = "Brave" },
                    new AgentPreset { Name = "Storm", Type = AgentType.BasicAI, Personality = "Brave" },
                    new AgentPreset { Name = "Phoenix", Type = AgentType.LLM, Personality = "Brave" }
                }
            },
            ["survival_experts"] = new TeamPreset
            {
                Name = "🛡️ Survival Experts",
                Description = "Cautious specialists who prioritize safety and risk assessment",
                Agents = new List<AgentPreset>
                {
                    new AgentPreset { Name = "Ranger", Type = AgentType.BasicAI, Personality = "Cautious" },
                    new AgentPreset { Name = "Medic", Type = AgentType.LLM, Personality = "Cautious" },
                    new AgentPreset { Name = "Engineer", Type = AgentType.BasicAI, Personality = "Logical" }
                }
            }
        };
    }
}

public class TeamPreset
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<AgentPreset> Agents { get; set; } = new();
}

public class AgentPreset
{
    public string Name { get; set; } = "";
    public AgentType Type { get; set; }
    public string Personality { get; set; } = "";
}

public enum AgentType
{
    BasicAI,
    LLM,
    Human
}
