# Framework for CMPM 121

This framework is for the class CMPM 121 - Game Development Patterns. It was developed in Unity version 6000.0.23f, but should also work in other Unity versions. 

## Artwork

All art used in this framework was released under CC-0. 

Spell and relic icons:
https://opengameart.org/content/dungeon-crawl-32x32-tiles

Roguelike Dungeon tiles:
https://opengameart.org/content/roguelike-caves-dungeons-pack
https://kenney.nl/assets/roguelike-caves-dungeons

UI:
https://kenney.nl/assets/ui-pack-pixel-adventure

Enemy sprites:
https://opengameart.org/content/tiny-creatures

Arcane bolt projectile:
https://opengameart.org/content/arcane-magic-effect

----------------------------------------------------------------------------------------------------------------------------
# CMPM 121 Assignment 2 README - Calvin Richards and Claire Buck

## Overview of the objective of assignments 2-4 (taken from the assignment page)

Genre - Rogue-like/Survival (similar to what we did in 120)

Core pillars:
    - Spell casting (flexible spell system)
    - Collecting relics (powerups)
    - Fighting against incoming [waves of] monsters

The key goal for this (and others) assignment is to write well-structured code.

Responsibility of each module is clear, code dealing with different aspects of the game is separated,
it is possible to use the developed code in a flexible manner, and it is easy to change behavior,
information/data is easy to change and not entangled with the code (data oriented design).

## Assignment 2:

Enemies will spawn in waves. Different difficulty modes (easy, medium, and endless) will have effects 
on both enemy numbers and stats. 

Parts:
    - Different types of enemies
    - Spawn these enemies as defined by the chosen difficulty level
    - Advance the game when the player has beaten a wave

Something important to remember: you do not need to worry about enemy behavior after spawning, there is
a rudimentary AI system in place. Spawn an instance of the enemy prefab and populate its values!

Focus on the definition of the different types and difficulty levels. 

Key files:
    - Resources/enemies.json
    - Resources/levels.json

What I (Calvin) have decided makes the most sense is to implement the following C# files within the 
"Levels" subdirectory of Assets/Scripts: Enemy.cs, Spawn.cs, and Level.cs

We are tasked with replacing the existing code inside of EnemySpawner.cs! GameManager.cs tracks the
number of enemies alive (useful for figuring out when a wave is over).

There should be wave "start" buttons between the waves, alongside info about the players stats 
between the waves (similar to an old arcade game). 

When the player dies or beats all the waves (unless in endless), restart the game. 

We must define AT LEAST one new enemy type. 

I have created submission.json (no commit yet), I have not yet created report.pdf

**Edit:
Doing a small change to this readme to test edit from a different device
