using RogueSharp_MonoGame.Core;
using Microsoft.Xna.Framework;
using RogueSharp_MonoGame.Monster;
using RogueSharp.DiceNotation;

namespace RogueSharp_MonoGame.Systems
{
    public class MapGenerator
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _maxRooms;
        private readonly int _roomMinSize;
        private readonly int _roomMaxSize;

        private readonly DungeonMap _map;

        public MapGenerator(int width, int height, int maxRooms, int roomMinSize, int roomMaxSize)
        {
            _width = width;
            _height = height;
            _map = new DungeonMap();
            _maxRooms = maxRooms;
            _roomMinSize = roomMinSize;
            _roomMaxSize = roomMaxSize;
            _map = new DungeonMap();
        }
        public DungeonMap CreateMap()
        {
            _map.Initialize(_width, _height);

            for (var r = _maxRooms; r > 0; r--)
            {
                var roomWidth = GameSession.Random.Next(_roomMinSize, _roomMaxSize);
                var roomHeight = GameSession.Random.Next(_roomMinSize, _roomMaxSize);
                var roomXPosition = GameSession.Random.Next(0, _width - roomWidth - 1);
                var roomYPosition = GameSession.Random.Next(0, _height - roomHeight - 1);

                var newRoom = new Rectangle(roomXPosition, roomYPosition, roomWidth, roomHeight);

                var newRoomIntersects = _map.Rooms.Any(room => newRoom.Intersects(room));

                if (!newRoomIntersects)
                {
                    _map.Rooms.Add(newRoom);
                }
            }

            foreach ( var room in _map.Rooms)
            {
                CreateRoom(room);
            }

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

            PlaceMonsters();

            return _map;
        }

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
    }
}
