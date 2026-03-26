using RogueSharp;
using RogueSharp_MonoGame.Interfaces;
using RogueSharp_MonoGame.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Path = RogueSharp.Path;

namespace RogueSharp_MonoGame.Behavior
{
    public class StandardMoveAndAttack : IBehaviour
    {
        public bool Act(Core.Monster monster, CommandSystem commandSystem)
        {
            var dungeonMap = GameSession.DungeonMap;
            var player = GameSession.Player;
            var monsterFov = new FieldOfView(dungeonMap);

            if (!monster.TurnsAlerted.HasValue)
            {
                monsterFov.ComputeFov(monster.X, monster.Y, monster.Awareness, true);
                if(monsterFov.IsInFov(player.X, player.Y))
                {
                    GameSession.MessageLog.Add($"{monster.Name} is eager to fight {player.Name}");
                    monster.TurnsAlerted = 1;
                }
            }

            if (monster.TurnsAlerted.HasValue)
            {
                dungeonMap.SetIsWalkable(monster.X, monster.Y, true);
                dungeonMap.SetIsWalkable(player.X, player.Y, true);

                var pathFinder = new PathFinder(dungeonMap);
                Path path = null;

                try
                {
                    path = pathFinder.ShortestPath(dungeonMap.GetCell(monster.X, monster.Y), dungeonMap.GetCell( player.X, player.Y));
                }
                catch (PathNotFoundException )
                {
                    GameSession.MessageLog.Add($"{monster.Name} waits for a turn");
                }

                dungeonMap.SetIsWalkable(monster.X, monster.Y, false);
                dungeonMap.SetIsWalkable(player.X, player.Y, false);

                if (path != null)
                {
                    try
                    {
                        commandSystem.MoveMonster(monster, path.StepForward());
                    }
                    catch (NoMoreStepsException)
                    {
                        GameSession.MessageLog.Add($"{monster.Name} growls in frustration");
                    }
                }

                monster.TurnsAlerted++;

                if (monster.TurnsAlerted > 15)
                {
                    monster.TurnsAlerted = null;
                }
            }

            return true;
        }
    }
}
