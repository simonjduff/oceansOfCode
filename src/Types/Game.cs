using System.Text;

namespace Types
{
    using System;
    using System.Linq;
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
            
            CatBot catBot = new CatBot(startLocation);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine($"{startLocation.X} {startLocation.Y}");

            Random random = new Random();
            // var moveStrategy = new RandomMoveStrategy(random);
            var moveStrategy = new GreatestOptionsMoveStrategy(6);
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
                // To debug: Console.Error.WriteLine("Debug messages...");s

                var move = moveStrategy.GetMove(grid, catBot);
                catBot.Move(grid, move);

                Console.WriteLine(move.ToMove());
            }
        }
    }

    public static class Extensions
    {
        public static string ToMove(this MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Surface:
                    return "SURFACE";
                default:
                    return $"MOVE {(char)direction}";
            }
        }

        public static MoveDirection ToMove(this Direction direction)
        {
            char dir = (char) direction;
            return (MoveDirection) dir;
        }
    }

    public interface IMoveStrategy
    {
        MoveDirection GetMove(Grid grid, CatBot bot);
    }
    
    public class CatBot : Token
    {
        public IList<Position> History { get; } = new List<Position>();

        public CatBot(Position position) : base(position)
        {
            Move(position);
        }

        public bool IsHypotheticalMoveValid(Grid grid, MoveDirection direction, Position position)
        {
            if (direction == MoveDirection.Surface)
            {
                return true;
            }
            
            var newPosition = grid.CalculateMoveLocation(position, direction);

            if (!grid.IsPositionInBounds(newPosition))
            {
                return false;
            }
            
            if (History.Contains(newPosition))
            {
                return false;
            }

            return grid.ContentsAt(newPosition) == GridContent.Empty;
        }

        public bool IsValidMove(Grid grid, MoveDirection direction)
            => IsHypotheticalMoveValid(grid, direction, Position);
        
        public void Move(Grid grid, MoveDirection direction)
        {
            if (!IsValidMove(grid, direction))
            {
                throw new InvalidOperationException($"Invalid move {Position} {direction}");
            }

            if (direction == MoveDirection.Surface)
            {
                History.Clear();
            }

            Move(grid.CalculateMoveLocation(Position, direction));
        }

        public void Move(Position position)
        {
            if (History.Contains(position))
            {
                throw new InvalidOperationException($"Already visited {position}");
            }
            
            History.Add(position);
            Position = position;
        }
    }

    public class GreatestOptionsMoveStrategy : IMoveStrategy
    {
        private int _searchDepth;

        public GreatestOptionsMoveStrategy(int searchDepth)
        {
            _searchDepth = searchDepth;
        }
        
        public MoveDirection GetMove(Grid grid, CatBot bot)
        {
            TreeNode root = new TreeNode(grid, bot, _searchDepth);
            var result = root.Traverse(new[] {bot.Position}, null);
            Console.Error.WriteLine($"Found {result.Direction} with depth {result.Depth}");
            return result.Direction;
        }

        private class TreeNode
        {
            private int _depth;
            private readonly Grid _grid;
            private CatBot _bot;
            private static readonly Random Random = new Random();

            public TreeNode(Grid grid,
                CatBot bot,
                int maxDepth)
            {
                _bot = bot;
                _grid = grid;
                _depth = maxDepth;
            }

            public (MoveDirection Direction, int Depth) Traverse(Position[] path, MoveDirection? direction)
            {
                if (!direction.HasValue)
                {
                    Console.Error.WriteLine($"Direction is null. Must be root node");
                }
                
                if (direction.HasValue && path.Length > _depth)
                {
                    return (direction.Value, path.Length);
                }


                Position currentPosition = path[path.Length - 1];
                
                List<(MoveDirection Direction, int Depth)> results = new List<(MoveDirection Direction, int Depth)>(4);
                
                foreach (MoveDirection dir in Enum.GetValues(typeof(MoveDirection)))
                {
                    var newLocation = _grid.CalculateMoveLocation(currentPosition, dir);
                    if (!path.Contains(newLocation)
                        && _bot.IsHypotheticalMoveValid(_grid, dir, currentPosition))
                    {
                        Position[] newPath = new Position[path.Length + 1];
                        path.CopyTo(newPath, 0);
                        newPath[newPath.Length - 1] = newLocation;
                        results.Add(Traverse(newPath, dir));
                    }
                }

                if (!results.Any() && !direction.HasValue)
                {
                    return (MoveDirection.Surface, path.Length);
                }
                
                if (!results.Any())
                {
                    return (direction.Value, path.Length);
                }

                var orderedResults = results.OrderByDescending(r => r.Depth);
                var depth = orderedResults.First().Depth;

                if (direction.HasValue)
                {
                    return (direction.Value, depth);
                }

                var best = orderedResults
                    .Where(r => r.Depth == depth).ToArray();

                return best[Random.Next(0, best.Length)];
            }
        }
    }

    public class RandomMoveStrategy : IMoveStrategy
    {
        private Random _random;

        public RandomMoveStrategy(Random random)
        {
            _random = random;
        }
        
        public MoveDirection GetMove(Grid grid, CatBot bot)
        {
            MoveDirection move;
            do
            {
                switch (_random.Next(0, 4))
                {
                    case 0: 
                        move = MoveDirection.North;
                        break;
                    case 1: 
                        move = MoveDirection.South;
                        break;
                    case 2: 
                        move = MoveDirection.East;
                        break;
                    case 3: 
                        move = MoveDirection.West;
                        break;
                    default: throw new InvalidOperationException("This is bug 1");
                }
                Console.Error.WriteLine($"Trying move {move}");
            } while (!bot.IsValidMove(grid, move));
            Console.Error.WriteLine($"Choosing move {move}");
            return move;
        }
    }
    
    public enum GridContent
    {
        Empty,
        Island
    }

    public class Token
    {
        protected Token(Position position)
        {
            Position = position;
        }
        
        public Position Position { get; protected set; }
    }

    public class Grid
    {
        private readonly GridContent[] _contents;
        public int Width { get; }
        public int Height { get; }

       public Grid(string[] input)
        {
            if (input.GroupBy(i => i.Length).Count() > 1)
            {
                throw new ArgumentException("Input is not of equal lengths");
            }
            
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

        private int PositionToIndex(Position position) => position.Y * Width + position.X;

        public bool IsPositionInBounds(Position position)
        => position.X >= 0 && position.Y >= 0
            && position.X < Width && position.Y < Height;

        public Position CalculateMoveLocation(Position position, MoveDirection direction)
        {
            Position newPosition;
            switch (direction)
            {
                case MoveDirection.North:
                    newPosition = new Position(position.X, position.Y - 1);
                    break;
                case MoveDirection.South:
                    newPosition = new Position(position.X, position.Y + 1);
                    break;
                case MoveDirection.East:
                    newPosition = new Position(position.X + 1, position.Y);
                    break;
                case MoveDirection.West:
                    newPosition = new Position(position.X - 1, position.Y);
                    break;
                case MoveDirection.Surface:
                    newPosition = position;
                    break;
                default: throw new NotImplementedException();
            }

            return newPosition;
        }

        public IEnumerable<Position> Neighbours(Position position)
        {
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                var newPosition = CalculateMoveLocation(position, dir.ToMove());
                if (IsPositionInBounds(newPosition))
                {
                    yield return newPosition;
                }
            }
        }
    }

    public enum Direction
    {
        North = 'N',
        South = 'S',
        East = 'E',
        West = 'W'
    }

    public enum MoveDirection
    {
        North = 'N',
        South = 'S',
        East = 'E',
        West = 'W',
        Surface = 0x00
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
