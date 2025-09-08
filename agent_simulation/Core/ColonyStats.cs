using System;
using AgentSimulation.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace AgentSimulation.Core;

public class ColonyStats
{
    public int DefenseRating { get; private set; }
    public int MedicalCapability { get; private set; }
    public int ResourceEfficiency { get; private set; }
    public int CommunicationRange { get; private set; }
    public int ResearchCapability { get; private set; }
    public double LifeSupportEfficiency { get; private set; } = 1.0; // Multiplier for life support benefits

    // Life support thresholds that provide bonuses
    public static readonly int HIGH_LIFE_SUPPORT_THRESHOLD = 80;
    public static readonly int MODERATE_LIFE_SUPPORT_THRESHOLD = 50;
    public static readonly int LOW_LIFE_SUPPORT_THRESHOLD = 20;

    public void UpdateStats(List<SimulationTask> tasks, int currentLifeSupport)
    {
        // Calculate defense rating based on completed defensive tasks
        DefenseRating = CalculateDefenseRating(tasks);
        
        // Calculate medical capability
        MedicalCapability = CalculateMedicalCapability(tasks);
        
        // Calculate resource efficiency
        ResourceEfficiency = CalculateResourceEfficiency(tasks);
        
        // Calculate communication range
        CommunicationRange = CalculateCommunicationRange(tasks);
        
        // Calculate research capability
        ResearchCapability = CalculateResearchCapability(tasks);
        
        // Calculate life support efficiency based on current level
        LifeSupportEfficiency = CalculateLifeSupportEfficiency(currentLifeSupport);
    }

    private int CalculateDefenseRating(List<SimulationTask> tasks)
    {
        int defense = 0;
        
        // Base defense from fortifications
        var fortifyTask = tasks.FirstOrDefault(t => t.Name.Contains("Fortify") || t.Name.Contains("Defense"));
        if (fortifyTask != null && fortifyTask.IsCompleted)
            defense += 30;
        else if (fortifyTask != null)
            defense += (int)(fortifyTask.Progress * 30.0 / fortifyTask.RequiredProgress);
        
        // Weapons contribute to defense
        var weaponTask = tasks.FirstOrDefault(t => t.Name.Contains("Weapon") || t.Name.Contains("Secure"));
        if (weaponTask != null && weaponTask.IsCompleted)
            defense += 25;
        else if (weaponTask != null)
            defense += (int)(weaponTask.Progress * 25.0 / weaponTask.RequiredProgress);
        
        // Combat clearing contributes
        var combatTask = tasks.FirstOrDefault(t => t.Type == TaskType.Combat);
        if (combatTask != null && combatTask.IsCompleted)
            defense += 20;
        else if (combatTask != null)
            defense += (int)(combatTask.Progress * 20.0 / combatTask.RequiredProgress);
        
        // Trained personnel contribute
        var trainingTask = tasks.FirstOrDefault(t => t.Name.Contains("Train"));
        if (trainingTask != null && trainingTask.IsCompleted)
            defense += 15;
        else if (trainingTask != null)
            defense += (int)(trainingTask.Progress * 15.0 / trainingTask.RequiredProgress);
        
        return Math.Max(0, defense);
    }

    private int CalculateMedicalCapability(List<SimulationTask> tasks)
    {
        int medical = 0;
        
        var medicalTasks = tasks.Where(t => t.Type == TaskType.Medical || t.Name.Contains("Medical") || t.Name.Contains("Treat")).ToList();
        foreach (var task in medicalTasks)
        {
            if (task.IsCompleted)
                medical += 20;
            else
                medical += (int)(task.Progress * 20.0 / task.RequiredProgress);
        }
        
        return Math.Max(0, medical);
    }

    private int CalculateResourceEfficiency(List<SimulationTask> tasks)
    {
        int efficiency = 0;
        
        var resourceTasks = tasks.Where(t => t.Type == TaskType.Resource).ToList();
        foreach (var task in resourceTasks)
        {
            if (task.IsCompleted)
                efficiency += 15;
            else
                efficiency += (int)(task.Progress * 15.0 / task.RequiredProgress);
        }
        
        return Math.Max(0, efficiency);
    }

    private int CalculateCommunicationRange(List<SimulationTask> tasks)
    {
        int range = 0;
        
        var commTasks = tasks.Where(t => t.Name.Contains("Communication") || t.Name.Contains("Radio")).ToList();
        foreach (var task in commTasks)
        {
            if (task.IsCompleted)
                range += 30;
            else
                range += (int)(task.Progress * 30.0 / task.RequiredProgress);
        }
        
        return Math.Max(0, range);
    }

    private int CalculateResearchCapability(List<SimulationTask> tasks)
    {
        int research = 0;
        
        var researchTasks = tasks.Where(t => t.Type == TaskType.Research || t.Name.Contains("Research") || t.Name.Contains("Study")).ToList();
        foreach (var task in researchTasks)
        {
            if (task.IsCompleted)
                research += 25;
            else
                research += (int)(task.Progress * 25.0 / task.RequiredProgress);
        }
        
        return Math.Max(0, research);
    }

    private double CalculateLifeSupportEfficiency(int currentLifeSupport)
    {
        if (currentLifeSupport >= HIGH_LIFE_SUPPORT_THRESHOLD)
            return 1.3; // 30% bonus at high life support
        else if (currentLifeSupport >= MODERATE_LIFE_SUPPORT_THRESHOLD)
            return 1.1; // 10% bonus at moderate life support
        else if (currentLifeSupport >= LOW_LIFE_SUPPORT_THRESHOLD)
            return 1.0; // No bonus at low life support
        else
            return 0.8; // 20% penalty at critical life support
    }

    public int CalculateEventDamageReduction(int originalDamage, string eventType)
    {
        if (eventType.Contains("Attack") || eventType.Contains("Horde") || eventType.Contains("Zombie"))
        {
            // Defense rating reduces combat damage
            double reductionPercent = Math.Min(0.7, DefenseRating / 100.0); // Max 70% reduction
            int reducedDamage = (int)(originalDamage * (1.0 - reductionPercent));
            return Math.Max(1, reducedDamage); // Always at least 1 damage
        }
        
        if (eventType.Contains("Medical") || eventType.Contains("Infection") || eventType.Contains("Injury"))
        {
            // Medical capability reduces medical-related damage
            double reductionPercent = Math.Min(0.5, MedicalCapability / 100.0); // Max 50% reduction
            int reducedDamage = (int)(originalDamage * (1.0 - reductionPercent));
            return Math.Max(1, reducedDamage);
        }
        
        return originalDamage; // No reduction for other event types
    }

    public int CalculateTaskProgressBonus(TaskType taskType)
    {
        int bonus = 0;
        
        // Life support efficiency affects all tasks
        bonus += (int)((LifeSupportEfficiency - 1.0) * 10);
        
        switch (taskType)
        {
            case TaskType.Combat:
                bonus += DefenseRating / 10;
                break;
            case TaskType.Medical:
                bonus += MedicalCapability / 10;
                break;
            case TaskType.Resource:
                bonus += ResourceEfficiency / 10;
                break;
            case TaskType.Research:
                bonus += ResearchCapability / 10;
                break;
            case TaskType.Engineering:
                if (CommunicationRange > 20)
                    bonus += 2; // Better coordination helps engineering
                break;
            case TaskType.Survival:
                bonus += (int)(LifeSupportEfficiency * 5); // Survival benefits from life support systems
                break;
        }
        
        return Math.Max(0, bonus);
    }

    public string GetStatusSummary()
    {
        return $"Defense: {DefenseRating}, Medical: {MedicalCapability}, Resources: {ResourceEfficiency}, " +
               $"Communication: {CommunicationRange}, Research: {ResearchCapability}, " +
               $"Life Support Efficiency: {LifeSupportEfficiency:F1}x";
    }
}
