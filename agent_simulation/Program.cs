//using AgentSimulation.Core;
//using AgentSimulation.Scenarios;

//namespace AgentSimulation;

//internal class Program
//{
//    static void Main(string[] args)
//    {
//        Console.WriteLine("🎮 AGENT SIMULATION");
//        Console.WriteLine("==================");
//        Console.WriteLine("Welcome to the Agent Simulation! Work together with your team to survive challenging scenarios.");
//        Console.WriteLine();
        
//        // Let user select scenario
//        var scenarioDefinition = ScenarioSelector.SelectScenario();
        
//        // Let user create their team
//        var team = TeamBuilder.CreateTeam();
        
//        // Ask if they want to see tips
//        Console.WriteLine("Would you like to see gameplay tips? (y/n)");
//        var showTips = Console.ReadLine()?.ToLower();
//        if (showTips == "y" || showTips == "yes")
//        {
//            ShowGameplayTips();
//        }
        
//        Console.WriteLine("🚀 Starting simulation...");
//        Console.WriteLine(new string('=', 60));
//        Console.WriteLine();
        
//        var sim = new Simulation(scenarioDefinition, team, "http://localhost:8080");
//        sim.Run();
//    }
    
//    private static void ShowGameplayTips()
//    {
//        Console.WriteLine();
//        Console.WriteLine("💡 GAMEPLAY TIPS");
//        Console.WriteLine("===============");
//        Console.WriteLine("• 🏥 Higher life support = better task efficiency bonuses");
//        Console.WriteLine("• 🛡️  Completing defense tasks reduces damage from attacks");
//        Console.WriteLine("• 🔧 Maintenance tasks reduce life support decay");
//        Console.WriteLine("• 📊 Check colony stats to see your current capabilities");
//        Console.WriteLine("• ⚡ Choose effort levels wisely - higher effort = more progress");
//        Console.WriteLine("• 🎯 Focus on tasks that complement your team's strengths");
//        Console.WriteLine("• 🤝 Work together - some tasks benefit from previous completions");
//        Console.WriteLine();
//        Console.WriteLine("Press Enter to continue...");
//        Console.ReadLine();
//        Console.WriteLine();
//    }
//}
