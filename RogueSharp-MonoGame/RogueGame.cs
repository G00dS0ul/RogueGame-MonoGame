using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RogueSharp_MonoGame.Core;
using RogueSharp_MonoGame.Systems;
using RogueSharp.Random;

namespace RogueSharp_MonoGame
{
    public class RogueGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _tileSet;
        private KeyboardState _previousKeyboardState;
        private SpriteFont _font;
        private CommandSystem _commandSystem = new CommandSystem();
        private static int _mapLevel = 1;

        public static MessageLog MessageLog { get; private set; } = new MessageLog();

        public RogueGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        // Layout constants
        public const int MapWidth = 50;      // tiles
        public const int MapHeight = 30;     // tiles (reduced to leave room for message log)
        public const int TileSize = 8;
        public const int TileScale = 2;
        public const int MapPixelWidth = MapWidth * TileSize * TileScale;   // 800
        public const int MapPixelHeight = MapHeight * TileSize * TileScale; // 480
        public const int StatsWidth = 200;
        public const int MessageLogHeight = 270;

        protected override void Initialize()
        {
            // Screen: Map + Stats panel on right, Message log at bottom
            _graphics.PreferredBackBufferWidth = MapPixelWidth + StatsWidth;  // 1000
            _graphics.PreferredBackBufferHeight = MapPixelHeight + MessageLogHeight; // 630
            _graphics.ApplyChanges();

            GameSession.Random = new DotNetRandom();
            GameSession.MessageLog = new MessageLog();
            GameSession.SchedulingSystem = new SchedulingSystem();
            GameSession.CommandSystem = _commandSystem;

            var mapGenerator = new MapGenerator(MapWidth, MapHeight, 20, 5, 10, _mapLevel);
            GameSession.DungeonMap = mapGenerator.CreateMap();

            GameSession.SchedulingSystem.Add(GameSession.Player);
            _commandSystem.IsPlayerTurn = true;

            GameSession.MessageLog.Add($"Welcome to the dungeon, G00dS0ul!");
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _tileSet = Texture2D.FromFile(GraphicsDevice, "terminal8x8.png");

            _font = Content.Load<SpriteFont>("Font");
        }

        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (GameSession.Player != null && GameSession.DungeonMap != null)
            {
                if (GameSession.Player.Health <= 0)
                {
                    _previousKeyboardState = currentKeyboardState;
                    base.Update(gameTime);
                    return;
                }

                if (_commandSystem.IsPlayerTurn)
                {
                    var didPlayerAct = false;
                    Direction? direction = null;

                    if (currentKeyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down))
                    {
                        direction = Direction.Down;
                    }
                    else if (currentKeyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up))
                    {
                        direction = Direction.Up;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left))
                    {
                        direction = Direction.Left;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right))
                    {
                        direction = Direction.Right;
                    }

                    if (currentKeyboardState.IsKeyDown(Keys.OemPeriod) && _previousKeyboardState.IsKeyUp(Keys.OemPeriod))
                    {
                        if (GameSession.DungeonMap.CanMoveDownToNextLevel())
                        {
                            _mapLevel++;

                            var mapGenerator = new MapGenerator(MapWidth, MapHeight, 20, 5, 10, _mapLevel);
                            GameSession.DungeonMap = mapGenerator.CreateMap();

                            MessageLog.Add($"You descend into the dungeon... level {_mapLevel}.");

                            didPlayerAct = true;
                        }
                        else
                        {
                            MessageLog.Add("You can't go down here yet. Defeat all the monsters on this level first!");
                        }
                    }

                    if (currentKeyboardState.IsKeyDown(Keys.OemComma) && _previousKeyboardState.IsKeyUp(Keys.OemComma))
                    {
                        if (GameSession.DungeonMap.CanMoveUpToPreviousLevel())
                        {
                            if (_mapLevel == 1)
                            {
                                MessageLog.Add("You cannot go up, you are already on the first level!!!");
                            }
                            else
                            {
                                _mapLevel--;

                                var mapGenerator = new MapGenerator(MapWidth, MapHeight, 20, 5, 10, _mapLevel);
                                GameSession.DungeonMap = mapGenerator.CreateMap();

                                MessageLog.Add($"You ascend the stairs to level {_mapLevel}");

                                didPlayerAct = true;
                            }
                        }

                        else
                        {
                            MessageLog.Add("You aren't tanding on upward stairs.");
                        }
                    }

                    if (direction.HasValue)
                    {
                        didPlayerAct = _commandSystem.MovePlayer(direction);
                    }

                    if (didPlayerAct)
                    {
                        _commandSystem.EndPlayerTurn();
                    }

                }
                else
                {
                    _commandSystem.ActivateMonsters();
                }
                
            }

            _previousKeyboardState = currentKeyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Colors.FloorBackground);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            if (GameSession.DungeonMap != null)
            {
                GameSession.DungeonMap.Draw(_spriteBatch, _tileSet, _font);
            }

            if (GameSession.Player != null)
            {
                GameSession.Player.DrawStats(_spriteBatch, _font);
                var playerAscii = '@';
                var tileX = (playerAscii % 16) * 8;
                var tileY = (playerAscii / 16) * 8;  // Fixed: multiply by 8, not modulo
                var sourceRect = new Rectangle(tileX, tileY, 8, 8);

                var scale = 2;
                var destRect = new Rectangle(GameSession.Player.X * 8 * scale, GameSession.Player.Y * 8 * scale, 8 * scale, 8 * scale);

                _spriteBatch.Draw(_tileSet, destRect, sourceRect, Colors.Player);
            }

            if (GameSession.MessageLog != null)
            {
                GameSession.MessageLog.Draw(_spriteBatch, _font);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private Rectangle GetSourceRect(char character)
        {
            var asciiValue = (int)character;
            var tileX = (asciiValue % 16) * 8;
            var tileY = (asciiValue / 16) * 8;

            return new Rectangle(tileX, tileY, 8, 8);
        }
    }
}
