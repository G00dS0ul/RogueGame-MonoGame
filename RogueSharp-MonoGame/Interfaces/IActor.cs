namespace RogueSharp_MonoGame.Interfaces
{
    public interface IActor
    {
        #region Properties

        string Name { get; set; }
        int Awareness { get; set; }
        int Attack { get; set; }
        int AttackChance { get; set; }
        int Defense { get; set; }
        int DefenseChance { get; set; }
        int Gold { get; set; }
        int Health { get; set; }
        int MaxHealth { get; set; }
        int Speed { get; set; }

        #endregion


    }
}
