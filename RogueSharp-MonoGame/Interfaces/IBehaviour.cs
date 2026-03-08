using System;
using System.Collections.Generic;
using System.Text;
using RogueSharp_MonoGame.Core;
using RogueSharp_MonoGame.Systems;
using RogueSharp;

namespace RogueSharp_MonoGame.Interfaces
{
    public interface IBehaviour
    {
        bool Act(Core.Monster monster, CommandSystem commandSystem);
    }
}
