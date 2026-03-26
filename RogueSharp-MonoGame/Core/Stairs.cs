using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSharp;
using IDrawable = RogueSharp_MonoGame.Interfaces.IDrawable;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RogueSharp_MonoGame.Core
{
    public class Stairs : IDrawable
    {
        public Color Color { get; set; }
        public char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsUp { get; set; }

        public void Draw(SpriteBatch console, Texture2D tileset)
        {
        }

        public void Draw(SpriteBatch spriteBatch, IMap map, Texture2D tileset)
        {
            if (!map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            Symbol = IsUp ? '<' : '>';

            if (map.IsInFov(X, Y))
            {
                Color = Colors.Player;
            }
            else
            {
                Color = Colors.Floor;
            }

            var asciiValue = (int)Symbol;
            var tileX = (asciiValue % 16) * 8;
            var tileY = (asciiValue / 16) * 8;

            var sourceRect = new Rectangle(tileX, tileY, 8, 8);
            var scale = 2;
            var destRect = new Rectangle(X * 8 * scale, Y * 8 * scale, 8 * scale, 8 * scale);

            spriteBatch.Draw(tileset, destRect, sourceRect, Color);
        }
    }
}
