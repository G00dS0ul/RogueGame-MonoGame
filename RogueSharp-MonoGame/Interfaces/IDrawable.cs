using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueSharp_MonoGame.Interfaces
{
    public interface IDrawable
    {
        #region Properties

        public Color Color { get; set; }
        public Char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        #endregion


        void Draw(SpriteBatch console, Texture2D tileset);
    }
}
