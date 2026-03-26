using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using RogueSharp;
using IDrawable = RogueSharp_MonoGame.Interfaces.IDrawable;
namespace RogueSharp_MonoGame.Core
{
    public class Door : IDrawable
    {
        public bool IsOpen { get; set; }
        public Color Color { get; set; }
        public Color BackgroundColor { get; set; }
        public char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }  
        public Door()
        {
            Symbol = '+';
            Color = Colors.Door;
            BackgroundColor = Colors.DoorBackground;
        }

        public void Draw(SpriteBatch console, Texture2D tileset)
        {

        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tileset, IMap map, FieldOfView fieldOfView)
        {
            if (!map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            Symbol = IsOpen ? '-' : '+';
            if (fieldOfView.IsInFov(X, Y))
            {
                Color = Colors.DoorFov;
                BackgroundColor = Colors.DoorBackgroundFov;
            }
            else
            {
                
                Color = Colors.Door;
                BackgroundColor = Colors.DoorBackground;
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
