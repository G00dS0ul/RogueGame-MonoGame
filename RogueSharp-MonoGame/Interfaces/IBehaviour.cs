using RogueSharp_MonoGame.Systems;

namespace RogueSharp_MonoGame.Interfaces
{
    public interface IBehaviour
    {
        bool Act(Core.Monster monster, CommandSystem commandSystem);
    }
}
