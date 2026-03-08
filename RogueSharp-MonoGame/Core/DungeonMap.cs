using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSharp;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RogueSharp_MonoGame.Core
{
    public class DungeonMap : Map
    {
        private FieldOfView _fieldOfView;
        private readonly List<Monster> _monsters;

        public List<Rectangle> Rooms { get; set; }
        public int MonsterCount => _monsters.Count;

        public DungeonMap()
        {
            Rooms = [];
            _monsters = [];
        }
        public void Initialize(int width, int height)
        {
            base.Initialize(width, height);
            _fieldOfView = new FieldOfView(this);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tileset, SpriteFont font)
        {
            var scale = 2;
            foreach (var cell in GetAllCells())
            {
                if (!cell.IsExplored)
                {
                    continue;
                }

                var isActorCell = GetMonsterAt(cell.X, cell.Y) != null || (GameSession.Player?.X == cell.X && GameSession.Player?.Y == cell.Y);
                var isFloor = cell.IsWalkable || isActorCell;
                var symbol = isFloor ? '.' : '#';

                Color tint;

                if (_fieldOfView.IsInFov(cell.X, cell.Y))
                {
                    tint = cell.IsWalkable ? Colors.FloorBackgroundFov : Colors.WallBackgroundFov;
                }
                else
                {
                    tint = cell.IsWalkable ? Colors.FloorFov : Colors.WallFov;
                }

                var asciiValue = (int)symbol;
                var tileX = (asciiValue % 16) * 8;
                var tileY = (asciiValue / 16) * 8;

                var sourceRect = new Rectangle(tileX, tileY, 8, 8);

                var destRect = new Rectangle(cell.X * 8 * scale, cell.Y * 8 * scale, 8 * scale, 8 * scale);
                spriteBatch.Draw(tileset, destRect, sourceRect, tint);
            }

            var i = 0;

            foreach (var monster in _monsters)
            {
                if (!_fieldOfView.IsInFov(monster.X, monster.Y))
                {
                    continue;
                }

                var asciiValue = (int)monster.Symbol;
                var tileX = (asciiValue % 16) * 8;
                var tileY = (asciiValue / 16) * 8;

                var sourceRect = new Rectangle(tileX, tileY, 8, 8);
                var destRect = new Rectangle(monster.X * 8 * scale, monster.Y * 8 * scale, 8 * scale, 8 * scale);

                spriteBatch.Draw(tileset, destRect, sourceRect, Colors.KoboldColor);
                monster.Draw(spriteBatch, tileset);

                monster.DrawStats(spriteBatch, i, font);
                i++;
                
            }
        }

        public Monster? GetMonsterAt(int x, int y)
        {
            return _monsters.FirstOrDefault(m => m.X == x && m.Y == y);
        }

        public void AddPlayer(Player player)
        {
            GameSession.Player = player;
            SetCellProperties(player.X, player.Y, GetCell(player.X, player.Y).IsTransparent, false);
            UpdatePlayerFieldOfView();
            GameSession.SchedulingSystem.Add(player);
        }

        public void RemoveMonster(Monster monster)
        {
            _monsters.Remove(monster);
            SetIsWalkable(monster.X, monster.Y, true);
            GameSession.SchedulingSystem.Remove(monster);
        }

        public void AddMonster(Monster monster)
        {
            _monsters.Add(monster);

            SetIsWalkable(monster.X, monster.Y, false);
                GameSession.SchedulingSystem.Add(monster);
        }

        public Point? GetRandomWalkableLocationInRoom(Rectangle room)
        {
            if (DoesRoomHaveWalkableSpace(room))
            {
                for (var i = 0; i < 100; i++)
                {
                    var x = GameSession.Random.Next(1, room.Width - 2) + room.X;
                    var y = GameSession.Random.Next(1, room.Height - 2) + room.Y;
                    if (IsWalkable(x, y))
                    {
                        return new Point(x, y);
                    }
                }
            }

            return null;
        }

        public bool DoesRoomHaveWalkableSpace(Rectangle room)
        {
            for (var x = 1; x <= room.Width - 2; x++)
            {
                for (var y = 1; y <= room.Height - 2; y++)
                {
                    if (IsWalkable(x + room.X, y + room.Y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetConsoleSymbolForCell(SpriteBatch spriteBatch, Cell cell)
        {
            //if (!cell.IsExplored)
            //{
            //    return;
            //}

            //if (cell.IsInFov)
            //{
            //    if (cell.IsWalkable)
            //    {
            //        console.Set(cell.X, cell.Y, Colors.FloorFov, Colors.FloorBackgroundFov, '.');
            //    }
            //    else
            //    {
            //        console.Set(cell.X, cell.Y, Colors.WallFov, Colors.WallBackgroundFov, '#');
            //    }
            //}
            //else
            //{
            //    if (cell.IsWalkable)
            //    {
            //        console.Set(cell.X, cell.Y, Colors.FLoor, Colors.FloorBackground, '.');
            //    }
            //    else
            //    {
            //        console.Set(cell.X, cell.Y, Colors.Wall, Colors.WallBackground, '#');
            //    }
            //}
        }

        public void UpdatePlayerFieldOfView()
        {
            var player = GameSession.Player;

            var cellsInFov = _fieldOfView.ComputeFov(player.X, player.Y, player.Awareness, true);

            var mapChanged = false;

            foreach (var cell in cellsInFov)
            {
                if (!cell.IsExplored)
                {
                    SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
                    mapChanged = true;
                }
                
            }

            if (mapChanged)
            {
                _fieldOfView.ComputeFov(player.X, player.Y, player.Awareness, true);
            }
        }

        public bool SetActorPosition(Actor actor, int x, int y)
        {
            if (GetCell(x, y).IsWalkable)
            {
                SetIsWalkable(actor.X, actor.Y, true);
                actor.X = x;
                actor.Y = y;

                SetIsWalkable(actor.X, actor.Y, false);
                if (actor is Player)
                {
                    UpdatePlayerFieldOfView();
                }

                return true;
            }

            return false;
        }

        public void SetIsWalkable(int x, int y, bool isWalkable)
        {
            var cell = GetCell(x, y);
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
        }
    }
}
