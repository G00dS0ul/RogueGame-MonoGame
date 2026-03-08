using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueSharp_MonoGame.Systems
{
    public class MessageLog
    {
        private static readonly int _maxLines = 10;

        private readonly Queue<string> _messages;

        public MessageLog()
        {
            _messages = new Queue<string>();
        }

        public void Add(string message)
        {
            _messages.Enqueue(message);

            if (_messages.Count > _maxLines)
            {
                _messages.Dequeue();
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Draw message log below the map area
            var startY = RogueGame.MapPixelHeight + 10;
            var messages = _messages.ToArray();
            for (var i = 0; i < messages.Length; i++)
            {
                spriteBatch.DrawString(font, messages[i], new Vector2(10, startY + (i * 18)), Color.White);
            }
        }
    }
}
