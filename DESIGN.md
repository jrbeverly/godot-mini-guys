# DESIGN — Autonomous Worker Base Defence MVP

This document decomposes [VISION.md](VISION.md) into a Godot-native architecture:
scenes, nodes, autoloads, signals, data structures, and implementation order.
It is the system plan that future issues reference.

## Game Phases

The game is always in exactly one phase. Transitions happen via `GameState`.

```
[Setup] → [Preparation] ⇄ [Wave]
              ↑              ↓
           [Won]  ←  (last wave cleared)
           [Lost] ←  (base destroyed)
```

| Phase | Entry condition | What happens |
|-------|----------------|--------------|
| **Setup** | Game start | Instantiate map, base, workers, rocks. Transition to `Preparation`. |
| **Preparation** | Setup or Wave ends | Player marks rocks, places build sites. Workers gather and build. Timer counts down to next wave. |
| **Wave** | Preparation timer expires | Enemies spawn at edges and move toward base. Defences auto-target. Phase ends when all enemies die or base is destroyed. |
| **Won** | Last wave cleared | Show victory UI. No further input. |
| **Lost** | Base health reaches 0 | Show defeat UI. No further input. |

`Preparation` and `Wave` alternate for a fixed number of waves (5 in MVP).
`Won` and `Lost` are terminal.

## Autoloads

Godot autoloads are singletons available from any script via `GetNode<Type>("/root/Name")`.

### GameState (`/root/GameState`)

Owns the authoritative game phase and stone count. Exposes signals for every
state change that other systems observe.

**State owned:**
- `Phase CurrentPhase` — one of `Setup`, `Preparation`, `Wave`, `Won`, `Lost`
- `int Stone` — player's stone balance
- `int WaveNumber` — current wave index (1-based)
- `int TotalWaves` — configured wave count (default 5)
- `float BaseHealth` — current central base HP
- `float BaseMaxHealth` — configured max HP

**Signals emitted:**
- `PhaseChanged(Phase newPhase)`
- `StoneChanged(int newTotal)`
- `WaveStarted(int waveNumber)`
- `WaveEnded(int waveNumber)`
- `BaseDamaged(float currentHealth)`
- `BaseDestroyed()`
- `AllWavesCleared()`

## Scene Tree

The main playfield scene (`Main.tscn`) is the single map.

```
Main (Node2D)
├── GameState          (autoload, not in scene tree)
├── Map (TileMapLayer or Node2D)
│   ├── Base (CharacterBody2D or StaticBody2D + script)
│   ├── Workers (container)
│   │   ├── Worker (CharacterBody2D + script)  — one per worker
│   │   └── ...
│   ├── Rocks (container)
│   │   ├── Rock (StaticBody2D + script)  — one per rock
│   │   └── ...
│   ├── BuildSites (container)
│   │   ├── BuildSite (Node2D + script)  — one per active build
│   │   └── ...
│   ├── Buildings (container)
│   │   ├── Wall (StaticBody2D + script)  — one per finished wall
│   │   ├── Tower (Area2D + script)       — one per finished tower
│   │   └── ...
│   ├── Enemies (container)
│   │   ├── Enemy (CharacterBody2D + script)  — one per active enemy
│   │   └── ...
│   └── SpawnPoints (Marker2D)  — enemy spawn positions
├── UI (CanvasLayer)
│   ├── StoneLabel
│   ├── WaveLabel
│   ├── BaseHealthBar
│   ├── BuildButtons (Wall, Tower)
│   └── PhaseBanner (Preparation / Wave / Won / Lost)
└── WaveController (Node + script)  — timer, spawn logic
```

Containers (`Workers`, `Rocks`, etc.) are plain `Node` or `Node2D` nodes used
for organisation. Scripts attach only to leaf game objects and the controller.

## Signal Contracts

Signals are the primary communication mechanism between subsystems. Each
signal is emitted by exactly one node type and consumed by one or more others.

| Signal | Emitter | Consumers | Payload |
|--------|---------|-----------|---------|
| `RockMarked` | Rock (on player click) | Worker (finds new job) | — (worker queries `GetTree()` or job queue) |
| `StoneDelivered` | Worker (arrives at base with stone) | GameState, UI | `int amount` |
| `BuildSitePlaced` | BuildSite (when player confirms placement) | Worker (finds new job) | — |
| `BuildProgressed` | Worker (contributes work to site) | BuildSite (updates progress) | `float workDone` |
| `BuildSiteCompleted` | BuildSite (when progress reaches required) | Buildings container (spawns finished building), UI | `BuildingType type, Vector2 position` |
| `EnemySpawned` | WaveController (spawns one enemy) | Enemies container | `Enemy instance` |
| `EnemyDied` | Enemy (health reaches 0) | WaveController (track remaining), GameState | — |
| `WaveStarted` | GameState | UI, WaveController | `int waveNumber` |
| `WaveEnded` | WaveController (all enemies dead) | GameState (advance phase), UI | `int waveNumber` |
| `BaseDamaged` | Base (takes damage) | GameState, UI | `float currentHealth` |
| `BaseDestroyed` | GameState (health reaches 0) | UI, WaveController | — |
| `AllWavesCleared` | GameState (last wave ended) | UI | — |
| `BuildingDamaged` | Wall / Tower (takes damage) | UI (optional) | `float currentHealth` |

All signals are declared in C# with `[Signal]` attribute:

```csharp
[Signal]
public delegate void StoneDeliveredEventHandler(int amount);
```

## Data Model

Data lives in C# records or plain classes attached to Godot nodes.

### Resource (record)

```csharp
public record StoneCount
{
    public int Amount { get; init; }
}
```

Held by `GameState`. Deducted on build-site placement, increased on
`StoneDelivered`.

### Job (record)

```csharp
public enum JobType { Gather, Build }

public record Job
{
    public required JobType Type { get; init; }
    public required Node2D Target { get; init; }  // Rock or BuildSite
}
```

Workers poll a job queue on `GameState` (or query `GetTree()` for marked
rocks and active build sites). This is a simple `List<Job>` managed by
`GameState`, not a complex system.

### BuildSite (class, extends Node2D)

```csharp
public enum BuildingType { Wall, Tower }

public partial class BuildSite : Node2D
{
    public BuildingType BuildingType { get; set; }
    public float RequiredWork { get; set; }
    public float WorkDone { get; set; }
    public int StoneCost { get; set; }
}
```

When `WorkDone >= RequiredWork`, emits `BuildSiteCompleted`.

### WaveConfig (record)

```csharp
public record WaveConfig
{
    public int WaveNumber { get; init; }
    public int EnemyCount { get; init; }
    public float SpawnIntervalSeconds { get; init; }
    public float PreparationTimeSeconds { get; init; }
    public float EnemySpeed { get; init; }
    public float EnemyHealth { get; init; }
}
```

A static array of 5 `WaveConfig` records defines all waves. Each successive
wave increases enemy count and speed.

### Health (record)

```csharp
public record Health
{
    public float Max { get; init; }
    public float Current { get; init; }
}
```

Attached as a field on Base, Wall, Tower, and Enemy scripts.

## Implementation Phases

Each phase is a milestone. Issues target one phase at a time.

### Phase 1 — Foundation

Scaffold the Godot project at `src/MiniGuys/`: `.csproj`, `.sln`,
`project.godot`, and the `Main` scene with a camera and empty node containers.
Add the `GameState` autoload with the `Phase` enum and `Stone` counter.
Write a `GameStateTests` class that validates phase transitions and stone
accounting.

### Phase 2 — Core World

Implement `Map`, `Base`, and `Rock` scenes as primitive shapes. Place them on
a fixed layout. The base has health and emits `BaseDamaged` / `BaseDestroyed`.
Rocks are static objects that the player can click to mark. Write unit tests
for base health and rock marking.

### Phase 3 — Gathering Loop

Implement the `Worker` scene: a small rectangle that can move toward a target,
play a brief gather animation (or timer), then return to the base and emit
`StoneDelivered`. Add idle workers finding marked rocks. Keep worker count
low (4–6). Write tests for worker state transitions and stone accumulation.

### Phase 4 — Build & Defences

Implement `BuildSite`, `Wall`, and `Tower`. The player spends stone to place
a build site at a chosen position. Workers contribute progress. When complete,
the build site is replaced by a finished wall or tower. Towers auto-detect and
fire at enemies. Write tests for build-site lifecycle and tower targeting.

### Phase 5 — Enemies & Waves

Implement `Enemy` and `WaveController`. Enemies spawn at map edges and move
toward the base, attacking obstacles in their path. The wave controller runs
the preparation/wave cycle for 5 waves with increasing difficulty. Write tests
for enemy pathing, wave progression, and win/loss conditions.

### Phase 6 — MVP Completion

Add UI (stone counter, wave indicator, base health bar, build buttons, phase
banner). Polish the Preparation→Wave→Recovery rhythm. Validate the full
playtest acceptance criteria from VISION. Fix bugs. No new systems.

## Non-Goals (from VISION)

These are explicitly excluded from the MVP. They may return later but must
not be started during the first six phases:

- Procedural generation — one fixed map only
- Multiple resources (wood, food, ore, etc.) — stone only
- Crafting chains, storage networks, production chains
- Worker equipment, specialised worker classes, worker upgrades
- Research trees or tech unlocks
- Save/load
- Complex pathfinding — enemies walk straight toward the base
- Large crowds — workers capped at ~12, enemies per wave at ~20 max
- Boss fights, multiple enemy types, flying or ranged enemies
- Digging, destructible terrain, dynamic terrain
- Multiple maps or biomes

## Related Documents

- [VISION.md](VISION.md) — game design, MVP scope, component behaviours
- [CODEMAP.md](CODEMAP.md) — repository structure, technology stack, C# conventions
- [README.md](README.md) — project overview

