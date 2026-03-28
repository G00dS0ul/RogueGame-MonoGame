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
        #region Backing Variable

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _tileSet;
        private InputSystem _inputSystem = new InputSystem();
        private SpriteFont _font;
        private CommandSystem _commandSystem = new CommandSystem();
        private static int _mapLevel = 1;
        private Dictionary<int, DungeonMap> _mapHistory = new Dictionary<int, DungeonMap>();

        #endregion

        #region Properties

        public static MessageLog MessageLog { get; private set; } = new MessageLog();


        #endregion

        public RogueGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        #region Layout Constants

        public const int MapWidth = 50;      // tiles
        public const int MapHeight = 30;     // tiles (reduced to leave room for message log)
        public const int TileSize = 8;
        public const int TileScale = 2;
        public const int MapPixelWidth = MapWidth * TileSize * TileScale;   // 800
        public const int MapPixelHeight = MapHeight * TileSize * TileScale; // 480
        public const int StatsWidth = 200;
        public const int MessageLogHeight = 270;

        #endregion

        #region Protected Methods

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
            _mapHistory.Add(_mapLevel, GameSession.DungeonMap);

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
            _inputSystem.Update();

            if (_inputSystem.IsExitRequested())
                Exit();

            if (GameSession.Player != null && GameSession.DungeonMap != null)
            {
                if (GameSession.Player.Health <= 0)
                {
                    base.Update(gameTime);
                    return;
                }

                if (_commandSystem.IsPlayerTurn)
                {
                    var didPlayerAct = false;

                    if (_inputSystem.IsDescendRequested())
                    {
                        if (GameSession.DungeonMap.CanMoveDownToNextLevel())
                        {
                            GameSession.DungeonMap.SetIsWalkable(GameSession.Player.X, GameSession.Player.Y, true);
                            _mapLevel++;

                            if (_mapHistory.TryGetValue(_mapLevel, out var savedMap))
                            {
                                GameSession.DungeonMap = savedMap; GameSession.DungeonMap.RestoreSchedulingSystem();
                            }

                            else
                            {
                                var mapGenerator = new MapGenerator(MapWidth, MapHeight, 20, 5, 10, _mapLevel);
                                GameSession.DungeonMap = mapGenerator.CreateMap();
                                _mapHistory.Add(_mapLevel, GameSession.DungeonMap);
                            }

                            GameSession.Player.X = GameSession.DungeonMap.StairsUp.X;
                            GameSession.Player.Y = GameSession.DungeonMap.StairsUp.Y;
                            GameSession.DungeonMap.AddPlayer(GameSession.Player);


                            MessageLog.Add($"You descend into the dungeon... level {_mapLevel}.");

                            didPlayerAct = true;
                        }
                        else
                        {
                            MessageLog.Add("You can't go down here yet. Defeat all the monsters on this level first!");
                        }
                    }

                    if (_inputSystem.IsAscendRequested())
                    {
                        if (GameSession.DungeonMap.CanMoveUpToPreviousLevel())
                        {
                            if (_mapLevel == 1)
                            {
                                MessageLog.Add("You cannot go up, you are already on the first level!!!");
                            }
                            else
                            {
                                GameSession.DungeonMap.SetIsWalkable(GameSession.Player.X, GameSession.Player.Y, true);

                                _mapLevel--;

                                if (_mapHistory.TryGetValue(_mapLevel, out var savedMap))
                                {
                                    GameSession.DungeonMap = savedMap;
                                    GameSession.DungeonMap.RestoreSchedulingSystem();
                                }
                                else
                                {
                                    var mapGenerator = new MapGenerator(MapWidth, MapHeight, 20, 5, 10, _mapLevel);
                                    GameSession.DungeonMap = mapGenerator.CreateMap();
                                    _mapHistory.Add(_mapLevel, GameSession.DungeonMap);
                                }


                                GameSession.Player.X = GameSession.DungeonMap.StairsDown.X;
                                GameSession.Player.Y = GameSession.DungeonMap.StairsDown.Y;
                                GameSession.DungeonMap.AddPlayer(GameSession.Player);

                                MessageLog.Add($"You ascend the stairs to level {_mapLevel}");

                                didPlayerAct = true;
                            }
                        }

                        else
                        {
                            MessageLog.Add("You aren't standing on upward stairs.");
                        }
                    }

                    var direction = _inputSystem.GetPlayerMovement();

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
                GameSession.Player.DrawStats(_spriteBatch, _font, _mapLevel);
                GameSession.Player.Draw(_spriteBatch, _tileSet);
            }

            if (GameSession.MessageLog != null)
            {
                GameSession.MessageLog.Draw(_spriteBatch, _font);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        #endregion

        #region Public Methods

        public static Rectangle GetSourceRect(char character)
        {
            var asciiValue = (int)character;
            var tileX = (asciiValue % 16) * 8;
            var tileY = (asciiValue / 16) * 8;

            return new Rectangle(tileX, tileY, 8, 8);
        }

        #endregion

    }
}
