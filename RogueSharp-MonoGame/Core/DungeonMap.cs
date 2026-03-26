using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework.Graphics;
using RogueSharp;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RogueSharp_MonoGame.Core
{
    public class DungeonMap : Map
    {
        private FieldOfView _fieldOfView;
        private readonly List<Monster> _monsters;
        private readonly List<Door> _doors;

        public List<Rectangle> Rooms { get; set; }
        public Stairs StairsUp { get; set; }
        public Stairs StairsDown { get; set; }  
        public int MonsterCount => _monsters.Count;

        public DungeonMap()
        {
            GameSession.SchedulingSystem.Clear();
            Rooms = [];
            _monsters = [];
            _doors = [];
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
                    tint = cell.IsWalkable ? Colors.FloorFov : Colors.WallFov;
                }
                else
                {
                    tint = cell.IsWalkable ? Colors.Floor : Colors.Wall;
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

            foreach (var door in _doors)
            {
                door.Draw(spriteBatch, tileset, this, _fieldOfView);
            }

            StairsUp?.Draw(spriteBatch, this,  tileset);
            StairsDown?.Draw(spriteBatch, this, tileset);
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

        public void AddMonster(Monster monster)
        {
            _monsters.Add(monster);

            SetIsWalkable(monster.X, monster.Y, false);
                GameSession.SchedulingSystem.Add(monster);
        }
        public void RemoveMonster(Monster monster)
        {
            _monsters.Remove(monster);
            SetIsWalkable(monster.X, monster.Y, true);
            GameSession.SchedulingSystem.Remove(monster);

            UpdateRoomDoorState();
        }

        public void AddDoor(Door door)
        {
            _doors.Add(door);
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

                    UpdateRoomDoorState();
                }

                return true;
            }

            OpenDoor(actor, x, y);

            return false;
        }

        public void SetIsWalkable(int x, int y, bool isWalkable)
        {
            var cell = GetCell(x, y);
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
        }

        public Door GetDoor(int x, int y)
        {
            return _doors.SingleOrDefault(d => d.X == x && d.Y == y);
        }

        private void OpenDoor(Actor actor, int x, int y)
        {
            var door = GetDoor(x, y);
            if (door != null && !door.IsOpen)
            {
                if (actor is Player)
                {
                    var currentRoom = Rooms.FirstOrDefault(r => r.Contains(actor.X, actor.Y));

                    if (currentRoom != default)
                    {
                        var enemiesInRoomCount = _monsters.Count(m => currentRoom.Contains(m.X, m.Y));
                        var roomBounds = new Rectangle(currentRoom.X - 1, currentRoom.Y - 1, currentRoom.Width + 2,
                            currentRoom.Height + 2);

                        if (enemiesInRoomCount > 0)
                        {
                            GameSession.MessageLog.Add(
                                $"The Door is Locked!!! Defect {enemiesInRoomCount} Monster in this room first.");
                            return;
                        }
                    }
                }
                door.IsOpen = true;
                var cell = GetCell(x, y);
                SetCellProperties(x, y, true, true, cell.IsExplored);
                
                GameSession.MessageLog.Add($"{actor.Name} opens the door.");
            }
        }

        public void UpdateRoomDoorState()
        {
            var player = GameSession.Player;
            if (player == null) return;

            var currentRoom = Rooms.FirstOrDefault(r => r.Contains(player.X, player.Y));

            if (currentRoom != default)
            {
                var enemiesInRoomCount = _monsters.Count(m => currentRoom.Contains(m.X, m.Y));

                var RoomBounds = new Rectangle(currentRoom.X - 1, currentRoom.Y - 1, currentRoom.Width + 2, currentRoom.Height + 2);
                var roomsDoors = _doors.Where(d => RoomBounds.Contains(d.X, d.Y)).ToList();

                if (enemiesInRoomCount > 0)
                {
                    var ambushTriggered = false;
                    foreach (var door in roomsDoors)
                    {
                        if (door.IsOpen)
                        {
                            door.IsOpen = false;
                            var cell = GetCell(door.X, door.Y);
                            SetCellProperties(door.X, door.Y, false, false, cell.IsExplored);
                            ambushTriggered = true;
                        }
                    }

                    if (ambushTriggered)
                    {
                        GameSession.MessageLog.Add("The Doors are Slam Shut! It's an Ambush");
                    }
                }
                else
                {
                    var doorsOpened = false;
                    foreach (var door in roomsDoors)
                    {
                        if (!door.IsOpen)
                        {
                            door.IsOpen = true;
                            var cell = GetCell(door.X, door.Y);
                            SetCellProperties(door.X, door.Y, true, true, cell.IsExplored);
                            doorsOpened = true;
                        }
                    }

                    if (doorsOpened)
                    {
                        GameSession.MessageLog.Add($"The room is cleared and doors unlock.");
                    }
                }
            }
        }

        public bool CanMoveUpToPreviousLevel()
        {
            var player = GameSession.Player;
            return StairsUp != null && StairsUp.X == player.X && StairsUp.Y == player.Y;
        }

        public bool CanMoveDownToNextLevel()
        {
            var player = GameSession.Player;
            return StairsDown != null && StairsDown.X == player.X && StairsDown.Y == player.Y;
        }
    }
}
