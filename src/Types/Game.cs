using System;
using System.Data;
using System.Net.Mime;

namespace Types
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    class Player
    {
        static void Main(string[] args)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int width = int.Parse(inputs[0]);
            int height = int.Parse(inputs[1]);
            int myId = int.Parse(inputs[2]);

            string[] gridInput = new string[height];
            
            for (int i = 0; i < height; i++)
            {
                gridInput[i] = Console.ReadLine();
            }
            
            Grid grid = new Grid(gridInput);
            
            StartLocator startLocator = new StartLocator();
            var startLocation = startLocator.FindLocation(grid);
            grid.SetPlayerPosition(startLocation);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine($"{startLocation.X} {startLocation.Y}");

            Random random = new Random();

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int myLife = int.Parse(inputs[2]);
                int oppLife = int.Parse(inputs[3]);
                int torpedoCooldown = int.Parse(inputs[4]);
                int sonarCooldown = int.Parse(inputs[5]);
                int silenceCooldown = int.Parse(inputs[6]);
                int mineCooldown = int.Parse(inputs[7]);
                string sonarResult = Console.ReadLine();
                string opponentOrders = Console.ReadLine();

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                Direction move;
                do
                {
                    switch (random.Next(0, 4))
                    {
                        case 0: 
                            move = Direction.North;
                            break;
                        case 1: 
                            move = Direction.South;
                            break;
                        case 2: 
                            move = Direction.East;
                            break;
                        case 3: 
                            move = Direction.West;
                            break;
                        default: throw new InvalidOperationException("This is bug 1");
                    }
                    Console.Error.WriteLine($"Trying move {move}");
                } while (!grid.IsValidMove(grid.PlayerPosition, move));
                Console.Error.WriteLine($"Choosing move {move}");
                grid.MovePlayer(move);
                
                Console.WriteLine($"MOVE {(char)move}");
            }
        }
    }
    
    public enum GridContent
    {
        Empty,
        Island,
        Traversed
    }

    public class Token
    {
        public Token(Position position)
        {
            Position = position;
        }
        
        public Position Position { get; protected set; }

        public void Place(Position position)
        {
            Position = position;
        }
    }

    public class Uboat : Token
    {
        public Uboat(Position position) : base(position)
        {
            Move(position);
        }

        public IList<Position> History { get; } = new List<Position>();

        public void Move(Position position)
        {
            Console.Error.WriteLine($"Trying to move me to {position}");
            
            if (History.Contains(position))
            {
                throw new InvalidOperationException($"Already visited {position}");
            }
            
            History.Add(position);
            Position = position;
        }
    }
    
    public class Grid
    {
        private readonly GridContent[] _contents;
        public int Width { get; }
        public int Height { get; }

        public Position PlayerPosition => _me.Position;

        private Uboat _me = null;

        public Grid(string[] input)
        {
            _contents = input.SelectMany(i => i.Select(c =>
                {
                    switch (c)
                    {
                        case '.': return GridContent.Empty;
                        case 'x': return GridContent.Island;
                        default:
                            throw new Exception($"Unknown input {c}");
                    }
                }) 
            ).ToArray();
            Width = input[0].Length;
            Height = input.Length;
        }

        public GridContent ContentsAt(Position position)
        {
            return _contents[PositionToIndex(position)];
        }

        public void SetPlayerPosition(Position position)
        {
            if (_me != null)
            {
                throw new InvalidOperationException("Cannot set the player twice");
            }

            _me = new Uboat(position);
        }

        private int PositionToIndex(Position position) => position.Y * Width + position.X;

        public bool IsValidMove(Position position, Direction direction)
        {
            var newPosition = CalculateMoveLocation(position, direction);

            if (!IsPositionValid(newPosition))
            {
                Console.Error.WriteLine($"Checking move... {newPosition} is invalid");
                return false;
            }

            if (_me.History.Contains(newPosition))
            {
                Console.Error.WriteLine($"Checking move... already been to {newPosition}");
                return false;
            }

            return _contents[PositionToIndex(newPosition)] == GridContent.Empty;
        }

        public bool IsPositionValid(Position position)
        => position.X >= 0 && position.Y >= 0
            && position.X < Width && position.Y < Height;

        public void MovePlayer(Direction direction)
        {
            if (!IsValidMove(_me.Position, direction))
            {
                throw new InvalidOperationException("Invalid move");
            }
            
            _me.Move(CalculateMoveLocation(_me.Position, direction));
        }

        private Position CalculateMoveLocation(Position position, Direction direction)
        {
            Position newPosition;
            switch (direction)
            {
                case Direction.North:
                    newPosition = new Position(position.X, position.Y - 1);
                    break;
                case Direction.South:
                    newPosition = new Position(position.X, position.Y + 1);
                    break;
                case Direction.East:
                    newPosition = new Position(position.X + 1, position.Y);
                    break;
                case Direction.West:
                    newPosition = new Position(position.X - 1, position.Y);
                    break;
                default: throw new NotImplementedException();
            }

            return newPosition;
        }
    }

    public enum Direction
    {
        North = 'N',
        South = 'S',
        East = 'E',
        West = 'W'
    }
        
    public class StartLocator
    {
        private static readonly Random _random = new Random();
        public Position FindLocation(Grid grid)
        {
            Position start;
            GridContent startContent;
            do
            {
                start = new Position(_random.Next(0, grid.Width), _random.Next(0, grid.Height));
                startContent = grid.ContentsAt(start);
            } while (startContent != GridContent.Empty);

            return start;
        }
    }

    public struct Position
    {
        public int X { get; }
        public int Y { get; }
        
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (!(obj is Position))
            {
                return false;
            }

            var other = (Position) obj;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = hash * 5 + X;
            hash = hash * 5 + Y;

            return hash;
        }

        public override string ToString()
            => $"({X},{Y})";
    }
}
