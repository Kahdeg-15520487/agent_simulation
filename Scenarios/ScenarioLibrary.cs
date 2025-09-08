using System.Collections.Generic;
using AgentSimulation.Events;
using AgentSimulation.Tasks;

namespace AgentSimulation.Scenarios;

public static class ScenarioLibrary
{
    public static ScenarioDefinition GetCrashedSpaceshipScenario()
    {
        var definition = new ScenarioDefinition(
            "Crashed Spaceship",
            "Your spaceship has crashed on an alien planet. You must repair it and maintain life support to survive."
        )
        {
            InitialLifeSupport = 100,
            LifeSupportDecay = 5,
            HoursPerStep = 2, // 2 hours per step
            WinCondition = "Complete all tasks before life support fails",
            LoseCondition = "Life support reaches 0"
        };

        definition.TaskDefinitions.AddRange(new[]
        {
            new TaskDefinition("Fix Engine", "Repair the spaceship engine to enable takeoff.", 100, TaskType.Engineering),
            new TaskDefinition("Gather Resources", "Collect materials from the planet for repairs.", 80, TaskType.Resource),
            new TaskDefinition("Maintain Life Support", "Ensure oxygen and power systems remain operational.", 60, TaskType.Maintenance)
        });

        // Add events
        var stormEvent = new EventDefinition("Alien Storm", "A massive storm approaches, threatening the ship.", EventType.Negative)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.15, // 15% chance per step
            IsOneTime = false
        };
        stormEvent.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, -20));
        stormEvent.Effects.Add(new EventEffect(EventEffect.EffectType.ChangeLifeSupportDecay, 2));
        definition.EventDefinitions.Add(stormEvent);

        var resourceDiscovery = new EventDefinition("Resource Discovery", "You find a cache of useful materials nearby.", EventType.Positive)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.1, // 10% chance per step
            IsOneTime = false
        };
        resourceDiscovery.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 25) { TaskName = "Gather Resources" });
        definition.EventDefinitions.Add(resourceDiscovery);

        var equipmentFailure = new EventDefinition("Equipment Failure", "Critical equipment breaks down.", EventType.Negative)
        {
            Trigger = EventTrigger.TimeBased,
            TriggerHour = 10, // Happens at hour 10
            IsOneTime = true
        };
        equipmentFailure.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, -30) { TaskName = "Fix Engine" });
        equipmentFailure.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, -15));
        definition.EventDefinitions.Add(equipmentFailure);

        return definition;
    }

    public static ScenarioDefinition GetZombieApocalypseScenario()
    {
        var definition = new ScenarioDefinition(
            "Zombie Apocalypse",
            "A zombie outbreak has occurred. You must fortify your shelter and find a cure."
        )
        {
            InitialLifeSupport = 80,
            LifeSupportDecay = 3,
            HoursPerStep = 4, // 4 hours per step (slower pace)
            WinCondition = "Find the cure and eliminate all zombies",
            LoseCondition = "Run out of supplies or get infected"
        };

        definition.TaskDefinitions.AddRange(new[]
        {
            new TaskDefinition("Fortify Shelter", "Build defenses against zombie attacks.", 120, TaskType.Engineering),
            new TaskDefinition("Find Cure", "Research and develop a zombie cure.", 150, TaskType.Research),
            new TaskDefinition("Gather Supplies", "Collect food, water, and medical supplies.", 90, TaskType.Resource),
            new TaskDefinition("Clear Zombies", "Eliminate zombies from the immediate area.", 100, TaskType.Combat)
        });

        // Add events
        var zombieHorde = new EventDefinition("Zombie Horde Attack", "A large group of zombies attacks your position!", EventType.Negative)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.2, // 20% chance per step
            IsOneTime = false
        };
        zombieHorde.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, -25));
        zombieHorde.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, -20) { TaskName = "Fortify Shelter" });
        definition.EventDefinitions.Add(zombieHorde);

        var medicalSupply = new EventDefinition("Medical Supply Cache", "You discover abandoned medical supplies.", EventType.Positive)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.12, // 12% chance per step
            IsOneTime = false
        };
        medicalSupply.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 30) { TaskName = "Find Cure" });
        medicalSupply.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, 10));
        definition.EventDefinitions.Add(medicalSupply);

        var survivorGroup = new EventDefinition("Survivor Group", "A group of survivors joins your camp.", EventType.Positive)
        {
            Trigger = EventTrigger.TimeBased,
            TriggerHour = 20, // Happens at hour 20
            IsOneTime = true
        };
        survivorGroup.Effects.Add(new EventEffect(EventEffect.EffectType.AddNewTask, 80) 
            { NewTaskName = "Train Survivors", NewTaskDescription = "Train the new survivors in combat and survival skills." });
        survivorGroup.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, 15));
        definition.EventDefinitions.Add(survivorGroup);

        return definition;
    }

    public static ScenarioDefinition GetSpaceStationScenario()
    {
        var definition = new ScenarioDefinition(
            "Space Station Crisis",
            "Your space station is malfunctioning. Multiple systems are failing simultaneously."
        )
        {
            InitialLifeSupport = 120,
            LifeSupportDecay = 7,
            WinCondition = "Restore all critical systems",
            LoseCondition = "Life support drops to 0 or station becomes uninhabitable"
        };

        definition.TaskDefinitions.AddRange(new[]
        {
            new TaskDefinition("Fix Oxygen System", "Repair the oxygen recycling system.", 80, TaskType.Maintenance),
            new TaskDefinition("Restore Power", "Fix the main power generator.", 100, TaskType.Engineering),
            new TaskDefinition("Seal Hull Breaches", "Repair structural damage to the station.", 90, TaskType.Engineering),
            new TaskDefinition("Reboot Computers", "Restart and reprogram station computers.", 70, TaskType.Engineering)
        });

        return definition;
    }

    public static List<ScenarioDefinition> GetAllScenarios()
    {
        return new List<ScenarioDefinition>
        {
            GetCrashedSpaceshipScenario(),
            GetZombieApocalypseScenario(),
            GetSpaceStationScenario()
        };
    }
}
