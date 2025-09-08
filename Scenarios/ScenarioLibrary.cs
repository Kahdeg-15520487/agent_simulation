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
            "A zombie outbreak has devastated the city. You and your team have barricaded yourselves in an abandoned research facility. Zombie hordes roam the streets, supplies are running low, and infected wounds could spell doom. You must find a cure, secure the area, and survive long enough to restore civilization - or at least your corner of it."
        )
        {
            InitialLifeSupport = 80,
            LifeSupportDecay = 3,
            HoursPerStep = 4, // 4 hours per step (slower pace for survival horror)
            WinCondition = "Find the cure, eliminate immediate zombie threats, and establish a secure safe zone",
            LoseCondition = "Run out of supplies, get overrun by zombies, or succumb to infection"
        };

        definition.TaskDefinitions.AddRange(new[]
        {
            new TaskDefinition("Fortify Shelter", "Build barricades, reinforce doors, and create defensive positions.", 120, TaskType.Engineering),
            new TaskDefinition("Find Cure", "Research zombie virus, analyze samples, and develop antidote.", 150, TaskType.Research),
            new TaskDefinition("Gather Medical Supplies", "Collect first aid kits, antibiotics, and medical equipment.", 90, TaskType.Resource),
            new TaskDefinition("Clear Immediate Area", "Eliminate zombies from the facility and surrounding grounds.", 100, TaskType.Combat),
            new TaskDefinition("Scavenge Food and Water", "Find canned goods, bottled water, and non-perishable supplies.", 80, TaskType.Resource),
            new TaskDefinition("Establish Communication", "Set up radio equipment to contact other survivor groups.", 60, TaskType.Engineering),
            new TaskDefinition("Treat Infected Wounds", "Provide medical care to prevent team members from turning.", 70, TaskType.Medical),
            new TaskDefinition("Secure Weapon Cache", "Find and secure firearms, ammunition, and melee weapons.", 50, TaskType.Resource),
            new TaskDefinition("Maintain Quarantine Protocol", "Isolate potentially infected areas and materials.", 40, TaskType.Maintenance)
        });

        // Enhanced event system with multiple zombie encounter types
        var zombieHorde = new EventDefinition("Zombie Horde Attack", "A massive horde of zombies breaks through your outer defenses!", EventType.Negative)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.18, // 18% chance per step
            IsOneTime = false
        };
        zombieHorde.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, -25));
        zombieHorde.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, -30) { TaskName = "Fortify Shelter" });
        zombieHorde.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 20) { TaskName = "Treat Infected Wounds" });
        definition.EventDefinitions.Add(zombieHorde);

        var fastZombieEncounter = new EventDefinition("Fast Zombie Pack", "A group of recently infected, fast-moving zombies attacks!", EventType.Negative)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.15, // 15% chance per step
            IsOneTime = false
        };
        fastZombieEncounter.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, -15));
        fastZombieEncounter.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, -20) { TaskName = "Clear Immediate Area" });
        definition.EventDefinitions.Add(fastZombieEncounter);

        var medicalSupplyCache = new EventDefinition("Medical Supply Cache", "You discover an abandoned ambulance with medical supplies.", EventType.Positive)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.12, // 12% chance per step
            IsOneTime = false
        };
        medicalSupplyCache.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 35) { TaskName = "Gather Medical Supplies" });
        medicalSupplyCache.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 25) { TaskName = "Find Cure" });
        medicalSupplyCache.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, 10));
        definition.EventDefinitions.Add(medicalSupplyCache);

        var survivorGroup = new EventDefinition("Survivor Group Arrival", "A group of survivors seeks refuge at your facility.", EventType.Positive)
        {
            Trigger = EventTrigger.TimeBased,
            TriggerHour = 20, // Happens at hour 20
            IsOneTime = true
        };
        survivorGroup.Effects.Add(new EventEffect(EventEffect.EffectType.AddNewTask, 80) 
            { NewTaskName = "Train New Survivors", NewTaskDescription = "Train the new arrivals in combat, survival, and security protocols." });
        survivorGroup.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, 20));
        survivorGroup.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 30) { TaskName = "Establish Communication" });
        definition.EventDefinitions.Add(survivorGroup);

        var infectedTeamMember = new EventDefinition("Team Member Infected", "One of your team members shows signs of zombie infection!", EventType.Negative)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.10, // 10% chance per step
            IsOneTime = false
        };
        infectedTeamMember.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 40) { TaskName = "Treat Infected Wounds" });
        infectedTeamMember.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, -10));
        infectedTeamMember.Effects.Add(new EventEffect(EventEffect.EffectType.ChangeLifeSupportDecay, 1));
        definition.EventDefinitions.Add(infectedTeamMember);

        var weaponStash = new EventDefinition("Police Station Raid", "You successfully raid an abandoned police station for weapons.", EventType.Positive)
        {
            Trigger = EventTrigger.TimeBased,
            TriggerHour = 16, // Happens at hour 16
            IsOneTime = true
        };
        weaponStash.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 45) { TaskName = "Secure Weapon Cache" });
        weaponStash.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 25) { TaskName = "Clear Immediate Area" });
        definition.EventDefinitions.Add(weaponStash);

        var foodShortage = new EventDefinition("Food Spoilage", "Some of your food supplies have spoiled due to power outages.", EventType.Negative)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.08, // 8% chance per step
            IsOneTime = false
        };
        foodShortage.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, -25) { TaskName = "Scavenge Food and Water" });
        foodShortage.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, -8));
        definition.EventDefinitions.Add(foodShortage);

        var scientistSurvivor = new EventDefinition("Scientist Survivor", "A virologist who worked on the original virus joins your group!", EventType.Positive)
        {
            Trigger = EventTrigger.TimeBased,
            TriggerHour = 28, // Happens at hour 28
            IsOneTime = true
        };
        scientistSurvivor.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 60) { TaskName = "Find Cure" });
        scientistSurvivor.Effects.Add(new EventEffect(EventEffect.EffectType.AddNewTask, 90) 
            { NewTaskName = "Mass Produce Antidote", NewTaskDescription = "Set up production facility to create cure for other survivor groups." });
        definition.EventDefinitions.Add(scientistSurvivor);

        var powerRestoration = new EventDefinition("Generator Repair", "You manage to restore partial power to the facility.", EventType.Positive)
        {
            Trigger = EventTrigger.TimeBased,
            TriggerHour = 12, // Happens at hour 12
            IsOneTime = true
        };
        powerRestoration.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, 15));
        powerRestoration.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 30) { TaskName = "Maintain Quarantine Protocol" });
        powerRestoration.Effects.Add(new EventEffect(EventEffect.EffectType.ChangeLifeSupportDecay, -1)); // Reduce decay with power
        definition.EventDefinitions.Add(powerRestoration);

        var mutantZombie = new EventDefinition("Mutant Zombie Encounter", "A horrifically mutated zombie breaks through your defenses!", EventType.Negative)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.06, // 6% chance per step (rare but devastating)
            IsOneTime = false
        };
        mutantZombie.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, -30));
        mutantZombie.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, -40) { TaskName = "Fortify Shelter" });
        mutantZombie.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 30) { TaskName = "Treat Infected Wounds" });
        mutantZombie.Effects.Add(new EventEffect(EventEffect.EffectType.AddNewTask, 120) 
            { NewTaskName = "Study Mutation Samples", NewTaskDescription = "Analyze the mutant zombie to understand virus evolution." });
        definition.EventDefinitions.Add(mutantZombie);

        return definition;
    }

    public static ScenarioDefinition GetSpaceStationScenario()
    {
        var definition = new ScenarioDefinition(
            "Space Station Crisis",
            "Your space station is malfunctioning. Multiple systems are failing simultaneously. A solar storm has damaged critical infrastructure, and you must restore all systems before the station becomes uninhabitable or drifts into a dangerous asteroid field."
        )
        {
            InitialLifeSupport = 120,
            LifeSupportDecay = 7,
            HoursPerStep = 3, // 3 hours per step
            WinCondition = "Restore all critical systems and stabilize the station",
            LoseCondition = "Life support drops to 0 or station becomes uninhabitable"
        };

        definition.TaskDefinitions.AddRange(new[]
        {
            new TaskDefinition("Fix Oxygen System", "Repair the oxygen recycling system damaged by the solar storm.", 80, TaskType.Maintenance),
            new TaskDefinition("Restore Primary Power", "Fix the main power generator and backup systems.", 120, TaskType.Engineering),
            new TaskDefinition("Seal Hull Breaches", "Repair structural damage to the station's outer hull.", 90, TaskType.Engineering),
            new TaskDefinition("Reboot Navigation", "Restart and reprogram navigation computers to avoid asteroid field.", 70, TaskType.Engineering),
            new TaskDefinition("Repair Communications", "Restore contact with Earth and nearby vessels.", 60, TaskType.Engineering),
            new TaskDefinition("Stabilize Life Support Core", "Fix the central life support processing unit.", 100, TaskType.Maintenance),
            new TaskDefinition("Secure Emergency Supplies", "Gather emergency rations, water, and medical supplies.", 50, TaskType.Resource),
            new TaskDefinition("Conduct Emergency Medical Care", "Treat injured crew members from the initial damage.", 40, TaskType.Medical)
        });

        // Add complex event system
        var solarFlare = new EventDefinition("Solar Flare", "Another solar flare hits the station, causing additional system failures.", EventType.Negative)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.18, // 18% chance per step
            IsOneTime = false
        };
        solarFlare.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, -15));
        solarFlare.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, -20) { TaskName = "Reboot Navigation" });
        solarFlare.Effects.Add(new EventEffect(EventEffect.EffectType.ChangeLifeSupportDecay, 2));
        definition.EventDefinitions.Add(solarFlare);

        var powerSurge = new EventDefinition("Power Surge", "Unstable power grid causes cascading failures across multiple systems.", EventType.Negative)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.15, // 15% chance per step
            IsOneTime = false
        };
        powerSurge.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, -25) { TaskName = "Restore Primary Power" });
        powerSurge.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, -15) { TaskName = "Fix Oxygen System" });
        definition.EventDefinitions.Add(powerSurge);

        var asteroidField = new EventDefinition("Asteroid Field Approach", "The station is drifting toward a dangerous asteroid field!", EventType.Negative)
        {
            Trigger = EventTrigger.TimeBased,
            TriggerHour = 15, // Happens at hour 15
            IsOneTime = true
        };
        asteroidField.Effects.Add(new EventEffect(EventEffect.EffectType.AddNewTask, 80) 
            { NewTaskName = "Emergency Navigation Override", NewTaskDescription = "Manually override navigation to avoid collision course." });
        asteroidField.Effects.Add(new EventEffect(EventEffect.EffectType.ChangeLifeSupportDecay, 3));
        definition.EventDefinitions.Add(asteroidField);

        var emergencyCache = new EventDefinition("Emergency Cache Discovery", "Crew finds a hidden emergency supply cache.", EventType.Positive)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.12, // 12% chance per step
            IsOneTime = false
        };
        emergencyCache.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 30) { TaskName = "Secure Emergency Supplies" });
        emergencyCache.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, 10));
        definition.EventDefinitions.Add(emergencyCache);

        var backupPowerOnline = new EventDefinition("Backup Power Activated", "Emergency backup systems come online, providing temporary relief.", EventType.Positive)
        {
            Trigger = EventTrigger.TimeBased,
            TriggerHour = 9, // Happens at hour 9
            IsOneTime = true
        };
        backupPowerOnline.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, 20));
        backupPowerOnline.Effects.Add(new EventEffect(EventEffect.EffectType.ChangeLifeSupportDecay, -2)); // Reduce decay temporarily
        definition.EventDefinitions.Add(backupPowerOnline);

        var crewInjury = new EventDefinition("Crew Injury", "A crew member is injured while conducting repairs in a dangerous area.", EventType.Negative)
        {
            Trigger = EventTrigger.Random,
            TriggerProbability = 0.10, // 10% chance per step
            IsOneTime = false
        };
        crewInjury.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyTaskProgress, 15) { TaskName = "Conduct Emergency Medical Care" });
        crewInjury.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, -5));
        definition.EventDefinitions.Add(crewInjury);

        var rescueSignal = new EventDefinition("Rescue Signal Received", "A nearby cargo ship responds to your distress call!", EventType.Positive)
        {
            Trigger = EventTrigger.TimeBased,
            TriggerHour = 24, // Happens at hour 24
            IsOneTime = true
        };
        rescueSignal.Effects.Add(new EventEffect(EventEffect.EffectType.AddNewTask, 60) 
            { NewTaskName = "Coordinate Rescue", NewTaskDescription = "Work with the rescue ship to establish safe docking procedures." });
        rescueSignal.Effects.Add(new EventEffect(EventEffect.EffectType.ModifyLifeSupport, 25));
        definition.EventDefinitions.Add(rescueSignal);

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
