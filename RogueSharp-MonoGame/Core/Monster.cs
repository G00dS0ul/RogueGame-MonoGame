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
        #region Backing Variable

        private static Texture2D? _pixel;

        #endregion

        #region Properties

        public int? TurnsAlerted { get; set; }

        #endregion

        #region Private Methods

        private static Texture2D GetPixel(SpriteBatch spriteBatch)
        {
            if (_pixel == null)
            {
                _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _pixel.SetData([Color.White]);
            }

            return _pixel;
        }

        private void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, int thickness, Color color)
        {
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);

        }

        #endregion

        #region Public Method

        public void DrawStats(SpriteBatch spriteBatch, int position, SpriteFont font, Texture2D tileset)
        {
            var startX = RogueGame.MapPixelWidth + 10;
            var startY = 190 + (position * 40);
            var barWidth = 150;
            var barHeight = 8;

            var textPosition = new Vector2(startX + 22, startY);

            var sourceRect = RogueGame.GetSourceRect(Symbol);
            var destRect = new Rectangle(startX, startY, 16, 16);
            spriteBatch.Draw(tileset, destRect, sourceRect, Color);

            var symbolSize = font.MeasureString($"{Symbol}: ");
            spriteBatch.DrawString(font, Name, textPosition, Colors.Text);

            var barY = startY + 20;
            var pixel = GetPixel(spriteBatch);
            spriteBatch.Draw(pixel, new Rectangle(startX, (int)barY, barWidth, barHeight), Swatch.DbGrass);

            var healthPercent = (float)Health / MaxHealth;
            var fillWidth = (int)(barWidth * healthPercent);
            spriteBatch.Draw(pixel, new Rectangle(startX, (int)barY, fillWidth, barHeight), Swatch.DbVegetation);

            DrawBorder(spriteBatch, pixel, new Rectangle(startX, (int)barY, barWidth, barHeight), 2, Swatch.AlternateLighter);

        }

        public virtual void PerformAction(CommandSystem commandSystem)
        {
            var behavior = new StandardMoveAndAttack();
            behavior.Act(this, commandSystem);
        }

        #endregion


       
    }
}
