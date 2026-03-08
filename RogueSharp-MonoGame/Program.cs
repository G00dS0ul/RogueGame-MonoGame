using System;
using System.Collections.Generic;
using System.Text;
using RogueSharp_MonoGame.Systems;

namespace RogueSharp_MonoGame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            var schedulingSystem = new SchedulingSystem();
            using (var game = new RogueGame())
                game.Run();

        }
    }
}
