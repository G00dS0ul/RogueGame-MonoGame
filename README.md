# 🗡️ Custom MonoGame Roguelike Engine

**Status:** Active Development  
**Tech Stack:** C#, .NET 10, MonoGame Framework, RogueSharp

## 📖 Overview
This repository contains the architecture for a custom, turn-based Roguelike game engine. This project serves as "V2" of my engine development, migrating from a standard console-based output to a fully decoupled rendering loop using the **MonoGame Framework**.

The primary goal of this project is not just to build a game, but to engineer robust, scalable software systems using deep Object-Oriented Programming (OOP) principles.

## ⚙️ Core Architecture & Features
* **Decoupled Rendering:** Separated the backend game logic (data state) from the visual layer. Entities dynamically draw their own sprites via the MonoGame `SpriteBatch` during the rendering loop.
* **Algorithmic Pathfinding & FOV:** Integrated robust algorithms for Field-of-View (FOV) calculations and monster pathfinding across grid-based maps.
* **Advanced State Management:** Implemented scheduling systems to handle turn-based execution cleanly, ensuring UI updates and game states remain perfectly synchronized.
* **Entity-Component Logic:** Built interfaces (`IActor`, `IDrawable`, `ISchedulable`) to enforce clean contracts across different game entities.

## 🚀 Technical Takeaways
Building this engine required translating raw mathematical coordinate logic into smooth visual rendering, while strictly managing memory and update loops, skills that map directly to high-level Desktop and UI software engineering.

## 🎮 How to Run
1. Clone the repository.
2. Ensure you have the [.NET 10 SDK](https://dotnet.microsoft.com/download) and [MonoGame](https://www.monogame.net/) installed.
3. Open the `.sln` in Visual Studio and build the project.
