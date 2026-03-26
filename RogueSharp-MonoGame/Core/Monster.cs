using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSharp_MonoGame.Behavior;
using RogueSharp_MonoGame.Systems;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace RogueSharp_MonoGame.Core
{
    public class Monster : Actor
    {
        private static Texture2D? _pixel;
        public int? TurnsAlerted { get; set; }

        private static Texture2D GetPixel(SpriteBatch spriteBatch)
        {
            if (_pixel == null)
            {
                _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _pixel.SetData([Color.White]);
            }

            return _pixel;
        }

        public void DrawStats(SpriteBatch spriteBatch, int position, SpriteFont font)
        {
            var startX = RogueGame.MapPixelWidth + 10;
            var startY = 150 + (position * 40);
            var barWidth = 150;
            var barHeight = 8;

            spriteBatch.DrawString(font, $"{Symbol}:", new Vector2(startX, startY), Color);

            var symbolSize = font.MeasureString($"{Symbol}: ");
            spriteBatch.DrawString(font, Name, new Vector2(startX + symbolSize.X, startY), Colors.Text);

            var barY = startY + 20;
            var pixel = GetPixel(spriteBatch);
            spriteBatch.Draw(pixel, new Rectangle(startX, (int)barY, barWidth, barHeight), Swatch.DbGrass);

            var healthPercent = (float)Health / MaxHealth;
            var fillWidth = (int)(barWidth * healthPercent);
            spriteBatch.Draw(pixel, new Rectangle(startX, (int)barY, fillWidth, barHeight), Swatch.DbVegetation);

            DrawBorder(spriteBatch, pixel, new Rectangle(startX, (int)barY, barWidth, barHeight), 1, Swatch.DbDark);

        }

        public virtual void PerformAction(CommandSystem commandSystem)
        {
            var behavior = new StandardMoveAndAttack();
            behavior.Act(this, commandSystem);
        }

        private void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, int thickness, Color color)
        {
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);

        }
    }
}
