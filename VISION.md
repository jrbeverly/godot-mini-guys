# Autonomous Worker Base Defence MVP Vision

## Overview

This project is a small **2D autonomous-worker base defence game** made in **Godot with C#**.

The player does not directly control individual units. Instead, the player shapes the settlement by choosing what should matter: which resources to gather, where to build, and how to prepare for enemy waves. The workers handle the moment-to-moment labour.

The core fantasy is:

> I am protecting a small base by guiding a group of tiny autonomous workers who gather, build, and react to danger.

The MVP should feel like a tiny living system, not a full colony simulator.

The goal is not to reproduce the full complexity of *Lil Guys*. The useful lesson from that type of game is that the hardest parts are scale, autonomy, collision, pathfinding, and avoiding excessive micromanagement. This MVP should deliberately avoid the “hundreds of agents with complex navigation” problem at first.

---

## Core Design Objective

The first version should answer one question:

> Is it fun to place simple intentions into the world and watch small autonomous workers turn those intentions into a defendable base?

Everything else is secondary.

The MVP should focus on a short, clear loop:

1. Gather stone.
2. Build defences.
3. Survive a wave.
4. Improve the base.
5. Survive several more waves.
6. Win or lose.

This is the full game at MVP scale.

---

## Game Identity

The game is not a city builder, not a deep colony sim, and not a traditional RTS.

It sits between three ideas:

- **Base defence**: enemies attack a central object.
- **Light automation**: workers perform tasks without direct control.
- **Simple building**: the player places planned structures that workers complete.

The game should be immediately readable. The player should understand what is happening by watching the world:

- workers walking to rocks;
- workers returning with stone;
- workers constructing build sites;
- towers firing at enemies;
- enemies approaching the base;
- the central base taking damage.

The game should feel active and alive without becoming complex.

---

## Player Role

The player is not a commander issuing detailed unit orders.

The player is more like a planner, caretaker, or settlement guide.

The player decides:

- which rocks should be harvested;
- where defences should be placed;
- whether to spend stone now or save it;
- how to prepare before the next enemy wave;
- how to recover after a wave.

The workers execute those intentions.

The player should not need to select individual workers, assign them one-by-one, or manage detailed logistics.

The ideal control model is:

> The player interacts with the world, and the workers respond.

---

## MVP Experience

A typical short session should feel like this:

The player starts with a small central base object, a handful of tiny workers, and several rocks nearby.

The player marks rocks for gathering. Workers move out, collect stone, and bring it back.

The player uses stone to place defensive structures. These appear first as unfinished build sites, not complete buildings. Workers then build them.

After a short preparation period, enemies spawn at the edge of the map and move toward the central base.

If the player prepared well, towers and walls slow or defeat the enemies. If not, the base takes damage.

After the wave, the player gets another preparation period and improves the base.

The game ends after a small number of waves.

Victory means the base survived.

Defeat means the central base was destroyed.

---

# Core Components

## 1. Central Base

The central base is the object the player is protecting.

It could be represented as a crown, core, crystal, shrine, heartstone, or tiny king-like object.

### Behavioural Requirements

The central base should:

- have health;
- be the main target for enemies;
- receive gathered resources from workers;
- clearly show when it is damaged or in danger;
- trigger defeat if destroyed.

The central base is the emotional centre of the game. It gives the player something to care about and gives enemies a clear objective.

---

## 2. Workers

Workers are the main charm of the game.

They should feel small, busy, and helpful. They do not need complicated intelligence. They only need to create the impression of a little colony doing useful work.

### Behavioural Requirements

Workers should:

- look for useful work;
- move toward a work target;
- gather from resources;
- return resources to the central base;
- help finish build sites;
- become idle when there is nothing useful to do.

The player should not need to select workers directly.

Idle workers should naturally find useful work based on the player’s intentions.

### Scale Limit

The first version should keep the worker count small.

A target range of **4 to 12 workers** is enough for the MVP.

The goal is not to simulate a huge crowd. The goal is to make a small number of agents feel readable and alive.

---

## 3. Resources

The MVP should have one resource:

> Stone

Stone is simple and readable. Rocks produce stone. Stone pays for defences.

### Behavioural Requirements

Resources should work like this:

- rocks exist on the map;
- the player can mark rocks for harvesting;
- workers gather stone from marked rocks;
- rocks eventually run out;
- the player can see how much stone they have.

There should be no multi-resource economy in the MVP.

Do not start with:

- wood;
- food;
- ore;
- tools;
- crafting chains;
- storage networks;
- production chains.

One resource is enough to test the core game.

---

## 4. Build Sites

Build sites are one of the most important concepts in the game.

When the player places a building, it should not instantly appear finished. Instead, the player places a **build site**.

The build site represents intention.

Workers then turn that intention into a finished structure.

### Behavioural Requirements

Build sites should:

- appear when the player chooses a building location;
- have a visible unfinished state;
- require worker effort to complete;
- show construction progress;
- become a real building when finished.

This makes workers feel meaningful.

The workers are not decorative. They are the reason the base physically comes together.

The core building chain is:

> Player intention → worker action → visible construction → stronger base

---

## 5. Defensive Buildings

The MVP only needs two building types:

- wall;
- tower.

These two buildings are enough to create a basic defensive strategy.

---

### Wall

The wall is a delay tool.

Its purpose is not to create a complex maze or advanced pathfinding puzzle. Its purpose is to buy time.

### Behavioural Requirements

Walls should:

- be built by workers;
- have health;
- be damaged by enemies;
- slow, block, or distract enemies;
- be destroyed if they take enough damage.

Walls should make base layout matter, but they should not require complicated navigation logic in the first version.

The simplest version is acceptable:

> Enemies move toward the base. If a wall gets in the way, they attack the wall.

---

### Tower

The tower is the simplest active defence.

### Behavioural Requirements

Towers should:

- be built by workers;
- detect nearby enemies;
- attack automatically;
- have a clear attack rhythm;
- give visible feedback when firing;
- help the player survive waves.

The tower should not require:

- ammo;
- power;
- operators;
- upgrades;
- targeting modes;
- line-of-sight rules.

Walls and towers should create a clear relationship:

> Walls buy time. Towers use that time.

---

## 6. Enemies

Enemies provide pressure.

The MVP only needs one enemy type.

### Behavioural Requirements

Enemies should:

- spawn during waves;
- move toward the central base;
- damage defensive objects if they are in the way;
- damage the central base if they reach it;
- die when they take enough damage;
- become more numerous or threatening over time.

Enemies should be simple, predictable, and readable.

Their purpose is to test whether the player prepared well.

Do not start with:

- multiple enemy types;
- bosses;
- ranged enemies;
- flying enemies;
- special abilities;
- complex pathfinding.

One basic enemy is enough.

---

## 7. Waves

Waves give the game structure.

Without waves, the player may not know what they are preparing for. With waves, the game has a rhythm:

> Prepare → attack → recover → prepare again

### Behavioural Requirements

The wave system should include:

- a preparation period;
- a visible warning before the next wave;
- enemy spawning when the wave begins;
- a clear wave end condition;
- increasing difficulty over time;
- victory after the final wave.

The MVP should probably have about **five waves**.

That is enough to create progression without requiring long-term balance.

---

## 8. Map

The MVP should use one fixed map.

### Behavioural Requirements

The map should include:

- a central base near the middle;
- rocks placed around the base;
- enemy spawn locations near the edges;
- enough open space for simple building;
- a readable play area.

The MVP should not include:

- procedural generation;
- digging;
- destructible terrain;
- dynamic terrain;
- multiple maps;
- biomes;
- infinite worlds.

A fixed map makes the game easier to build, easier to test, and easier to debug.

---

# Practical System Interactions

## Gathering Interaction

The player marks a rock.

That creates useful work in the world.

A worker notices that work is available. The worker goes to the rock, gathers stone, returns to the central base, and the player’s stone count increases.

The player should understand this by watching the action happen.

The desired chain is:

> Mark rock → worker travels → worker gathers → worker returns → stone increases

---

## Building Interaction

The player chooses a building and places it on the map.

A build site appears.

Workers treat the build site as useful work. They go to it and contribute labour. When enough labour has been contributed, the build site becomes a finished building.

The desired chain is:

> Choose building → place build site → workers build → finished defence appears

This is the heart of the base-building experience.

---

## Wave Interaction

The player gets a warning that a wave is coming.

Enemies appear at the map edges and move toward the central base.

Defensive buildings respond automatically.

If enemies break through, the central base takes damage.

When the wave ends, the player gets time to gather, build, and prepare again.

The desired chain is:

> Warning → enemy wave → defence test → recovery period

---

# Godot Usage at the Right Level

The Godot implementation should support the design without becoming the focus.

The project should use Godot’s strengths:

- scenes for reusable game objects;
- nodes for composition;
- C# scripts for behaviour;
- signals or events for important state changes;
- resources later for tunable data if needed.

At a high level, the game can be thought of as a set of reusable objects:

- worker;
- enemy;
- rock;
- build site;
- wall;
- tower;
- central base;
- wave controller;
- UI panel.

The important point is not the exact technical structure.

The important point is that each object has a clear gameplay purpose.

---

## C# Role in the Project

C# should be used because it matches the developer’s strengths.

The project should not try to force GDScript if C# is more natural and productive.

The practical goal is:

> Use C# to describe small behaviours attached to Godot objects.

For example:

- the worker knows how to perform work;
- the enemy knows how to advance and attack;
- the tower knows how to fire;
- the central base knows when it has been damaged;
- the wave system knows when to begin the next wave;
- the UI knows how to display current status.

The project should not start with:

- abstract frameworks;
- dependency injection;
- generic architecture layers;
- formal domain modelling;
- premature optimization;
- large reusable systems.

Those things can come later if the game earns them.

The first challenge is not architectural elegance.

The first challenge is making a tiny interactive simulation feel good.

---

# AI-Assisted Development Strategy

AI should be used to help implement behaviours after the behaviours are clearly defined.

The workflow should be:

1. Describe the gameplay behaviour in plain language.
2. Ask AI for the smallest Godot C# implementation that supports that behaviour.
3. Run it in Godot.
4. Paste back errors or unexpected behaviour.
5. Fix one thing at a time.
6. Avoid letting AI expand the feature.

Good AI prompts should sound like gameplay requirements, not architecture requests.

Example:

> In Godot 4 C#, I have a worker object and a rock object. I want the worker to move to a marked rock, wait briefly, reduce the rock’s remaining stone, then return to the base and increase the stone counter. Keep it simple and do not add extra systems.

Another example:

> In Godot 4 C#, I want a build site that appears when the player places a wall. Workers should be able to contribute progress to it. When progress is complete, the build site should become a finished wall. Keep this focused only on the build-site behaviour.

A poor AI prompt would be:

> Design a scalable colony sim architecture with jobs, resources, workers, waves, buildings, and combat.

That kind of prompt invites unnecessary complexity.

The AI should be treated like a fast assistant that helps build one behaviour at a time.

---

# MVP Boundaries

## The First Version Should Include

- one fixed map;
- one resource;
- one worker type;
- one enemy type;
- one central base;
- one wall;
- one tower;
- build sites;
- enemy waves;
- win and lose states;
- basic UI feedback.

## The First Version Should Not Include

- digging;
- procedural maps;
- multiple resources;
- crafting chains;
- worker equipment;
- specialized worker classes;
- research;
- save/load;
- complex pathfinding;
- large crowds;
- boss fights;
- upgrades;
- production chains;
- complex storage.

These exclusions are not permanent.

They protect the MVP.

---

# Practical Acceptance Criteria

The MVP is successful when a new player can:

- understand that the central base must be protected;
- mark rocks for gathering;
- see workers bring back stone;
- spend stone on defences;
- watch workers construct those defences;
- survive at least one enemy wave;
- understand why they won or lost.

That is the test.

Not code elegance.

Not system completeness.

Not future scalability.

The MVP is about proving the basic experience.

---

# Development Mindset

The first build should be treated like a playable toy.

The right question after each milestone is:

> Does this make the tiny base feel more alive, more readable, or more playable?

If the answer is no, the feature probably does not belong in the MVP.

The project should stay focused on visible, practical gameplay elements:

- workers moving;
- resources being gathered;
- buildings appearing;
- enemies approaching;
- defences responding;
- the base being threatened;
- the player making simple choices;
- the world visibly responding.

That is the target.

Everything technical should serve that.