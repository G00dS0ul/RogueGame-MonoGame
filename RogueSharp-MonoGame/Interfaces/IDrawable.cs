using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSharp;

namespace RogueSharp_MonoGame.Interfaces
{
    public interface IDrawable
    {
        public Color Color { get; set; }
        public Char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        void Draw(SpriteBatch console, Texture2D tileset);
    }
}
