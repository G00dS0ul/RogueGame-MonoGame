using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueSharp_MonoGame.Core
{
    public class Player : Actor
    {
        public Player()
        {
            Awareness = 10;
            Name = "G00DS0UL";
            Attack = 2;
            AttackChance = 50;
            Defense = 2;
            DefenseChance = 50;
            Gold = 0;
            Health = 100;
            MaxHealth = 100;
            Speed = 10;
            Color = Colors.Player;
            Symbol = '@';
            X = 10;
            Y = 10;
        }

        public void DrawStats(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Draw stats in the right panel (after the map area)
            var startX = RogueGame.MapPixelWidth + 10;
            var startY = 20;

            spriteBatch.DrawString(font, $"Name: {Name}", new Vector2(startX, startY), Color.White);
            spriteBatch.DrawString(font, $"Health: {Health}/{MaxHealth}", new Vector2(startX, startY + 20), Color.Green);
            spriteBatch.DrawString(font, $"Attack: {Attack} ({AttackChance}%)", new Vector2(startX, startY + 40), Color.Red);
            spriteBatch.DrawString(font, $"Defense: {Defense}({DefenseChance}%)", new Vector2(startX, startY + 60), Color.White);
            spriteBatch.DrawString(font, $"Gold: {Gold}", new Vector2(startX, startY + 80), Color.Yellow);

            var enemyCOunt = GameSession.DungeonMap?.MonsterCount ?? 0;
            spriteBatch.DrawString(font, $"Enemies Remaining: {enemyCOunt}", new Vector2(startX, startY + 100), Color.OrangeRed);
        }
    }
}
