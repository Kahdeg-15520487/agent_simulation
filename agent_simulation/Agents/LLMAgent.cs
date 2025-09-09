using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSimulation.Scenarios;

namespace AgentSimulation.Agents;

public class LLMAgent : Agent
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    public string Endpoint { get; set; }
    private int taskIndex = -1;

    public LLMAgent(string name, string personality, string model = "llama2", string endpoint = "http://localhost:5000")
        : base(name, personality)
    {
        _httpClient = new HttpClient();
        _model = model;
        Endpoint = endpoint;
    }

    public override string Think(Scenario scenario)
    {
        // Generate thought using LLM
        var raw = GenerateThoughtWithLLM(scenario).Result;
        var llmThought = JsonSerializer.Deserialize<LLMThought>(raw); // Synchronous for simplicity
        CurrentThought = llmThought.thought;
        taskIndex = llmThought.task_index;
        Memory.Add(CurrentThought);
        return CurrentThought;
    }

    private async Task<string> GenerateThoughtWithLLM(Scenario scenario)
    {
        try
        {
            var prompt = BuildPrompt(scenario);

            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"{Endpoint}/v1/chat/completions", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "I need to think...";
        }
        catch (Exception ex)
        {
            // Fallback to base implementation if LLM fails
            Console.WriteLine($"LLM error for {Name}: {ex.Message}");
            return GenerateThought(scenario);
        }
    }

    private string BuildPrompt(Scenario scenario)
    {
        var memoryText = Memory.Count > 0 ? string.Join("\n", Memory.TakeLast(3)) : "No previous thoughts.";

        var tasksText = string.Join("\n", scenario.Tasks.Select((t, i) => $"{i}. {t.Name}: {t.Description} (Progress: {t.Progress}%)"));

        var prompt = $@"
You are an agent named {Name} with a {Personality} personality in a simulation.

Scenario: {scenario.Name}
Life Support: {scenario.LifeSupport}%

Available Tasks:
task index | task name | task description | progress
{tasksText}

Your recent thoughts:
{memoryText}

As a {Personality} agent, what is your current thought about the situation and which task should you do now? Keep it concise, one sentence.
Respond in this format:
{{
    ""thought"": ""Your thought here"",
    ""task_index"": task_index_number
}}
";

        return prompt.Trim();
    }

    public override string Act(Scenario scenario, int _)
    {
        return base.Act(scenario, taskIndex);
    }

    class LLMThought
    {
        public string thought { get; set; } = "I need to think...";
        public int task_index { get; set; } = -1;
    }
}