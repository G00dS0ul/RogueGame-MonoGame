using RogueSharp_MonoGame.Core;
using RogueSharp_MonoGame.Monster;
using RogueSharp;
using RogueSharp.DiceNotation;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RogueSharp_MonoGame.Systems
{
    public class MapGenerator
    {
        #region Backing Variable

        private readonly int _width;
        private readonly int _height;
        private readonly int _maxRooms;
        private readonly int _roomMinSize;
        private readonly int _roomMaxSize;
        private readonly DungeonMap _map;

        #endregion

        public MapGenerator(int width, int height, int maxRooms, int roomMinSize, int roomMaxSize, int mapLevel)
        {
            _width = width;
            _height = height;
            _map = new DungeonMap();
            _maxRooms = maxRooms;
            _roomMinSize = roomMinSize;
            _roomMaxSize = roomMaxSize;
            _map = new DungeonMap();
        }

        #region Public Methods

        public DungeonMap CreateMap()
        {
            _map.Initialize(_width, _height);

            for (var r = _maxRooms; r > 0; r--)
            {
                var roomWidth = GameSession.Random.Next(_roomMinSize, _roomMaxSize);
                var roomHeight = GameSession.Random.Next(_roomMinSize, _roomMaxSize);
                var roomXPosition = GameSession.Random.Next(1, _width - roomWidth - 1);
                var roomYPosition = GameSession.Random.Next(1, _height - roomHeight - 1);

                var newRoom = new Rectangle(roomXPosition, roomYPosition, roomWidth, roomHeight);

                var newRoomIntersects = _map.Rooms.Any(room => newRoom.Intersects(room));

                if (!newRoomIntersects)
                {
                    _map.Rooms.Add(newRoom);
                }
            }

            foreach (var room in _map.Rooms)
            {
                CreateRoom(room);
            }

            CreateStairs();

            PlacePlayer();

            for (var r = 1; r < _map.Rooms.Count; r++)
            {
                var previousRoomCenterX = _map.Rooms[r - 1].Center.X;
                var previousRoomCenterY = _map.Rooms[r - 1].Center.Y;
                var currentRoomCenterX = _map.Rooms[r].Center.X;
                var currentRoomCenterY = _map.Rooms[r].Center.Y;

                if (GameSession.Random.Next(1, 2) == 1)
                {
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY);
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX);
                }
                else
                {
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, previousRoomCenterX);
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, currentRoomCenterY);
                }
            }

            foreach (var room in _map.Rooms)
            {
                CreateDoors(room);

            }

            PlaceMonsters();

            return _map;
        }

        #endregion

        #region Private Method

        private void CreateRoom(Rectangle room)
        {
            for (var x = room.Left; x < room.Right; x++)
            {
                for (var y = room.Top; y < room.Bottom; y++)
                {
                    var cell = _map.GetCell(x, y);
                    _map.SetCellProperties(x, y, true, true, true);
                }
            }
        }

        private void PlacePlayer()
        {
            var player = GameSession.Player;

            if (player == null)
            {
                player = new Player();
            }

            player.X = _map.Rooms[0].Center.X;
            player.Y = _map.Rooms[0].Center.Y;

            _map.AddPlayer(player);
        }

        private void PlaceMonsters()
        {
            foreach (var room in _map.Rooms)
            {
                if (Dice.Roll("1D10") < 7)
                {
                    var numberOfMonsters = Dice.Roll("1D4");
                    for (var i = 0; i < numberOfMonsters; i++)
                    {
                        Point? randomRoomLocation = _map.GetRandomWalkableLocationInRoom(room);

                        if (randomRoomLocation != null)
                        {
                            var monster = Kobold.Create(1);
                            monster.X = randomRoomLocation.Value.X;
                            monster.Y = randomRoomLocation.Value.Y;
                            _map.AddMonster(monster);
                        }
                    }
                }
            }
        }

        private void CreateHorizontalTunnel(int xStart, int xEnd, int yPosition)
        {
            for (var x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                _map.SetCellProperties(x, yPosition, true, true);
            }
        }

        private void CreateVerticalTunnel(int yStart, int yEnd, int xPosition)
        {
            for (var y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                _map.SetCellProperties(xPosition, y, true, true);
            }
        }

        private void CreateDoors(Rectangle room)
        {
            var xMin = room.Left - 1;
            var xMax = room.Right;
            var yMin = room.Top - 1;
            var yMax = room.Bottom;

            var borderCells = new List<ICell>();

            borderCells = _map.GetCellsAlongLine(xMin, yMin, xMax, yMin).ToList();
            borderCells.AddRange(_map.GetCellsAlongLine(xMin, yMin, xMin, yMax));
            borderCells.AddRange(_map.GetCellsAlongLine(xMin, yMax, xMax, yMax));
            borderCells.AddRange(_map.GetCellsAlongLine(xMax, yMin, xMax, yMax));

            foreach (var cell in borderCells)
            {
                if (IsPotentialDoor(cell))
                {
                    _map.SetCellProperties(cell.X, cell.Y, false, false);
                    _map.AddDoor(new Door
                    {
                        X = cell.X,
                        Y = cell.Y,
                        IsOpen = false
                    });
                }
            }
        }

        private bool IsPotentialDoor(ICell cell)
        {
            if (!cell.IsWalkable)
            {
                return false;
            }

            if (cell.X <= 0 || cell.X >= _map.Width - 1 || cell.Y <= 0 || cell.Y >= _map.Height - 1)
            {
                return false;
            }

            var right = _map.GetCell(cell.X + 1, cell.Y);
            var left = _map.GetCell(cell.X - 1, cell.Y);
            var top = _map.GetCell(cell.X, cell.Y + 1);
            var bottom = _map.GetCell(cell.X, cell.Y - 1);

            if (_map.GetDoor(cell.X, cell.Y) != null || _map.GetDoor(right.X, right.Y) != null ||
                _map.GetDoor(left.X, left.Y) != null || _map.GetDoor(top.X, top.Y) != null ||
                _map.GetDoor(bottom.X, bottom.Y) != null)
            {
                return false;
            }

            if (!right.IsWalkable && !left.IsWalkable && top.IsWalkable && bottom.IsWalkable)
            {
                return true;
            }

            if (right.IsWalkable && left.IsWalkable && !top.IsWalkable && !bottom.IsWalkable)
            {
                return true;
            }

            return false;
        }

        private void CreateStairs()
        {
            _map.StairsUp = new Stairs
            {
                X = _map.Rooms.First().Center.X + 1,
                Y = _map.Rooms.First().Center.Y,
                IsUp = true
            };
            _map.StairsDown = new Stairs
            {
                X = _map.Rooms.Last().Center.X,
                Y = _map.Rooms.Last().Center.Y,
                IsUp = false
            };
        }

        #endregion

    }
}
