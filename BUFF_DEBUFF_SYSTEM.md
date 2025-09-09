# Buff and Debuff System Documentation

## Overview

The Agent Simulation now includes a comprehensive buff and debuff system that allows scenario effects to dynamically modify task performance, life support mechanics, and overall simulation behavior. This system provides rich gameplay dynamics where task completion and random events can create lasting impacts on the simulation.

## Core Components

### 1. ScenarioEffect Class (`BuffDebuffSystem.cs`)

The `ScenarioEffect` class represents individual buffs and debuffs with the following properties:

- **Name**: Display name of the effect
- **Description**: Detailed description of what the effect does
- **Type**: Buff or Debuff (EffectType enum)
- **Target**: What the effect affects (EffectTarget enum)
- **Multiplier**: Multiplicative modifier (e.g., 1.3 = 30% increase, 0.7 = 30% decrease)
- **FlatValue**: Additive modifier (e.g., +5 progress per action)
- **Duration**: How long the effect lasts (-1 = permanent, positive numbers = steps remaining)
- **Source**: What created this effect (task name, event, etc.)

### 2. EffectManager Class

The `EffectManager` handles all active effects and provides methods to:

- Add new effects
- Remove effects
- Update effect durations each turn
- Calculate combined modifiers for tasks
- Get life support bonuses/penalties
- Display effect summaries

### 3. Effect Targets

Effects can target different aspects of the simulation:

- **TaskType**: Affects all tasks of a specific type (Engineering, Medical, etc.)
- **SpecificTask**: Affects one particular task by name
- **AllTasks**: Affects every task in the scenario
- **LifeSupport**: Directly modifies life support values
- **LifeSupportDecay**: Changes the rate of life support decay

## Integration Points

### Task Completion Actions

Tasks can now add effects when completed using fluent syntax:

```csharp
new TaskDefinition("Assess Ship Damage", "Survey the crash site...", 40, TaskType.Engineering)
    .AddsBuffToTaskType("Engineering Expertise", "Detailed assessment provides insight", TaskType.Engineering, 1.3, 10)
    .AddsLifeSupportDecayReduction("Efficient Systems", "Optimized systems reduce power consumption", 0.8, 15)
```

Available helper methods:
- `AddsBuffToTaskType()` - Buff all tasks of a specific type
- `AddsDebuffToTaskType()` - Debuff all tasks of a specific type  
- `AddsBuffToSpecificTask()` - Buff a specific task
- `AddsLifeSupportBuff()` - Add flat life support bonus
- `AddsLifeSupportDecayReduction()` - Reduce life support decay rate

### Event System Integration

Events can also create effects using the new `AddScenarioEffect` event type:

```csharp
equipmentFailure.Effects.Add(new EventEffect(EventEffect.EffectType.AddScenarioEffect)
{
    EffectName = "System Instability",
    EffectDescription = "Malfunctioning systems make engineering work more difficult",
    EffectTypeStr = "Debuff",
    EffectTarget = "TaskType",
    EffectTargetTaskType = TaskType.Engineering,
    EffectMultiplier = 0.7, // 30% reduction in engineering progress
    EffectDuration = 8 // Lasts 8 steps
});
```

### Agent Task Performance

When agents work on tasks, the system now:

1. Calculates base progress (random 5-15)
2. Adds colony stat bonuses
3. Applies effect flat bonuses
4. Applies effect multipliers
5. Reports detailed breakdown to the user

Example output:
```
Alex worked on Fix Engine:
  Progress: 45 → 62/120 (+17: +12 base, +2 colony bonus, +3 effect bonus, ×1.3 effect multiplier)
```

## Example Scenarios

### Engineering Buff from Assessment

When "Assess Ship Damage" is completed:
- Adds "Engineering Expertise" buff
- All Engineering tasks get 30% progress bonus for 10 steps
- Represents gained knowledge making future engineering work easier

### System Malfunction Debuff

"System Malfunction" event triggers:
- Adds "System Instability" debuff  
- All Engineering tasks suffer 30% progress penalty for 8 steps
- Represents damaged systems being harder to work with

### Resource Efficiency Buff

"Mineral Deposits Found" event:
- Adds "Abundant Resources" buff
- All Resource tasks get 50% progress bonus for 6 steps
- Represents easier material gathering from rich deposits

### Life Support Optimization

"Fix Life Support system" completion:
- Adds life support decay reduction (20% decrease in decay rate)
- Effect lasts 15 steps
- Represents more efficient life support systems

## UI Integration

The simulation dashboard now includes an "Effects" panel that displays:
- Active effect names with buff/debuff icons (↗️/↘️)
- Effect types and targets
- Remaining duration
- Color coding (green for buffs, red for debuffs)

## Technical Implementation

### Agent Progress Calculation

The `ActOnTask` method in `Agent.cs` now applies effect modifiers:

```csharp
// Apply effect multipliers and bonuses
var effectMultiplier = scenario.EffectManager.GetTaskProgressMultiplier(task);
var effectBonus = scenario.EffectManager.GetTaskProgressBonus(task);

// Calculate final progress
var totalBaseProgress = baseProgress + bonusProgress + effectBonus;
var finalProgress = (int)Math.Round(totalBaseProgress * effectMultiplier);
```

### Scenario Updates

Each simulation step:
1. Updates effect durations and removes expired effects
2. Applies life support bonuses/penalties from effects
3. Modifies life support decay based on effect multipliers
4. Processes new effects from completed tasks and triggered events

## Usage Examples

### Creating Task-Based Effects

```csharp
new TaskDefinition("Research Alien Technology", "Study alien devices", 80, TaskType.Research)
    .AddsBuffToTaskType("Tech Insight", "Understanding alien tech helps engineering", TaskType.Engineering, 1.4, 12)
    .AddsDebuffToTaskType("Resource Drain", "Research consumes materials", TaskType.Resource, 0.8, 6)
```

### Creating Event-Based Effects

```csharp
var solarFlare = new EventDefinition("Solar Flare", "Electromagnetic interference affects systems", EventType.Negative);
solarFlare.Effects.Add(new EventEffect(EventEffect.EffectType.AddScenarioEffect)
{
    EffectName = "EM Interference", 
    EffectDescription = "Electronics malfunction intermittently",
    EffectTypeStr = "Debuff",
    EffectTarget = "AllTasks",
    EffectMultiplier = 0.9,
    EffectDuration = 5
});
```

This system creates rich emergent gameplay where past actions influence future performance, random events create temporary challenges or opportunities, and players must consider the long-term strategic implications of their task priorities and completion timing.
