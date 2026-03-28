using Microsoft.Xna.Framework.Input;
using RogueSharp_MonoGame.Core;

namespace RogueSharp_MonoGame.Systems
{
    public class InputSystem
    {
        #region Backing Variable

        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;

        #endregion

        #region Public Method

        public void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
        }

        public Direction? GetPlayerMovement()
        {
            if(IsKeyPressed(Keys.Up)) return Direction.Up;
            if(IsKeyPressed(Keys.Down)) return Direction.Down;
            if(IsKeyPressed(Keys.Left)) return Direction.Left;
            if(IsKeyPressed(Keys.Right)) return Direction.Right;

            return null;
        }

        #endregion

        #region Private Method

        private bool IsKeyPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        }

        #endregion

        #region Adstract Command

        public bool IsExitRequested() => _currentKeyboardState.IsKeyDown(Keys.Escape);
        public bool IsDescendRequested() => IsKeyPressed(Keys.OemPeriod);
        public bool IsAscendRequested() => IsKeyPressed(Keys.OemComma);

        #endregion
        
    }
}
