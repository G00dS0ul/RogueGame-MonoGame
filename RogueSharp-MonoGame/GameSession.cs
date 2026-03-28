using RogueSharp_MonoGame.Core;
using RogueSharp_MonoGame.Systems;
using RogueSharp.Random;

namespace RogueSharp_MonoGame
{
    public static class GameSession
    {

        public static Player Player { get; set; }
        public static DungeonMap DungeonMap { get; set; }
        public static MessageLog MessageLog { get; set; }
        public static CommandSystem CommandSystem { get; set; }
        public static IRandom Random { get; set; }
        public static SchedulingSystem SchedulingSystem { get; set; }

    }
}
