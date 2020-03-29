using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

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
        private static Regex _moveRegex = new Regex("MOVE (?<move>[NSEW])", RegexOptions.Compiled);
        
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
            
            Grid grid = new Grid(new GridInputToBytes(), gridInput);
            
            StartLocator startLocator = new StartLocator();
            var startLocation = startLocator.FindLocation(grid);
            
            CatBot catBot = new CatBot(startLocation);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine($"{startLocation.X} {startLocation.Y}");

            Random random = new Random();
            // var moveStrategy = new RandomMoveStrategy(random);
            var moveStrategy = new GreatestOptionsMoveStrategy(6);
            var torpedoStrategy = new TorpedoStrategy(random, grid, catBot);
            var locatorStrategy = new BasicEnemyLocatorStrategy(grid);
            List<Direction> enemyMoves = new List<Direction>();
            // game loop
            int turnNumber = 0;
            while (true)
            {
                turnNumber++;
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

                var moveMatch = _moveRegex.Match(opponentOrders);
                if (moveMatch.Success)
                {
                    enemyMoves.Add((Direction)moveMatch.Groups["move"].Value[0]);
                }
                Console.Error.WriteLine($"Spotted moves: {string.Join(" ", enemyMoves)}");

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");s

                var timer = new Stopwatch();
                timer.Start();
                locatorStrategy.LocateEnemy(enemyMoves.ToArray());
                
                timer.Stop();
                Console.Error.WriteLine($"Enemy locator took {timer.ElapsedMilliseconds}ms");
                
                var move = moveStrategy.GetMove(grid, catBot);
                catBot.Move(grid, move);
                string torpedo;
                if (turnNumber % 4 != 0)
                {
                    torpedo = " TORPEDO";
                }
                else
                {
                    var target = torpedoStrategy.GetTarget();
                    torpedo = $"|TORPEDO {target.X} {target.Y}";
                }

                Console.Error.WriteLine($"Torpedo is {torpedo}");
                Console.WriteLine($"{move.ToMove()}{torpedo}");
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

    public class BasicEnemyLocatorStrategy
    {
        private readonly Grid _grid;

        public BasicEnemyLocatorStrategy(Grid grid)
        {
            _grid = grid;
        }

        public IEnumerable<Position> LocateEnemy(IEnumerable<Direction> moves)
        {
            int northCount = 0;
            int southCount = 0;
            int westCount = 0;
            int eastCount = 0;
            foreach (var move in moves)
            {
                switch (move)
                {
                    case Direction.East:
                        eastCount++;
                        break;
                    case Direction.North:
                        northCount++;
                        break;
                    case Direction.South:
                        southCount++;
                        break;
                    case Direction.West:
                        westCount++;
                        break;
                }
            }

            var positions = FindPositions(moves, westCount, northCount);
            uint[] myGrid = new uint[_grid.GridBinary.Length];
            foreach (var position in positions)
            {
                myGrid[position.Y] = myGrid[position.Y] | (uint) Math.Pow(2, _grid.Width - 1 - position.X);
            }


            uint[] gridShape = new uint[_grid.GridBinary.Length];
            _grid.GridBinary.CopyTo(gridShape, 0);

            uint mask = ~(uint)Enumerable.Range(0, _grid.Width).Sum(i => (uint) Math.Pow(2, i));

            for (int i = 0; i < gridShape.Length; i++)
            {
                gridShape[i] = gridShape[i] | mask;
            }

            List<Position> offsetMatches = new List<Position>();

            Position offset = new Position(0,0);
            uint[] search = new uint[myGrid.Length];
            myGrid.CopyTo(search, 0);
            do
            {
                StringBuilder sb = new StringBuilder();
                foreach (var row in search)
                {
                    sb.AppendLine(Convert.ToString(row, 2).PadLeft(5, '0'));
                }

                string result = sb.ToString();

                bool fail = false;
                for (int row = 0; row < gridShape.Length; row++)
                {
                    if ((gridShape[row] & search[row]) != 0)
                    {
                        fail = true;
                        break;
                    }
                }
                
                if (!fail)
                {
                    offsetMatches.Add(offset);
                    // Console.Error.WriteLine($"Found a place the enemy could be hiding offset {offset}");
                }
                
                if (search.Any(row => (row & 1) != 0) && (search[search.Length - 1] & ~(uint) 0) != 0)
                {
                    break;
                }

                if (search.All(row => (row & 1) == 0))
                {
                    for (int i = 0; i < search.Length; i++)
                    {
                        search[i] = search[i] >> 1;
                    }
                    offset = new Position(offset.X + 1, offset.Y);
                    continue;
                }

                
                uint temp = myGrid[myGrid.Length - 1];
                for (int i = myGrid.Length - 1; i > 0; i--)
                {
                    myGrid[i] = myGrid[i - 1];
                }
                myGrid[0] = temp;
                myGrid.CopyTo(search, 0);
                offset = new Position(0, offset.Y + 1);

            } while (true);

            Position lastPosition = positions[positions.Count - 1];
            return offsetMatches.Select(m => new Position(m.X + lastPosition.X, m.Y + lastPosition.Y));
        }

        private List<Position> FindPositions(IEnumerable<Direction> moves, int westCount, int northCount)
        {
            List<Position> positions = new List<Position>();
            Position currentPosition = new Position(westCount, northCount);
            positions.Add(currentPosition);
            foreach (var move in moves)
            {
                Position newPosition;
                switch (move)
                {
                    case Direction.East:
                        newPosition = new Position(currentPosition.X + 1, currentPosition.Y);
                        break;
                    case Direction.North:
                        newPosition = new Position(currentPosition.X, currentPosition.Y - 1);
                        break;
                    case Direction.South:
                        newPosition = new Position(currentPosition.X, currentPosition.Y + 1);
                        break;
                    case Direction.West:
                        newPosition = new Position(currentPosition.X - 1, currentPosition.Y);
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid move {move}");
                }

                positions.Add(newPosition);
                currentPosition = newPosition;
            }

            return positions;
        }
    }

    public class GreatestOptionsMoveStrategy : IMoveStrategy
    {
        private readonly int _searchDepth;

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
        public uint[] GridBinary { get; }
        public int Width { get; }
        public int Height { get; }
        
        public int CellCount { get; }

       public Grid(GridInputToBytes gridInputToBytes, string[] input)
        {
            if (input.GroupBy(i => i.Length).Count() > 1)
            {
                throw new ArgumentException("Input is not of equal lengths");
            }

            GridBinary = gridInputToBytes.GetInt64(input);
            
            Width = input[0].Length;
            Height = input.Length;
            CellCount = Width * Height;
        }

        public GridContent ContentsAt(Position position)
        {
            int reversedIndex = Width - 1 - position.X;
            uint mask = (uint) Math.Pow(2, reversedIndex);

            var row = GridBinary[position.Y];

            uint result = mask & row;
            return result == 0 ? GridContent.Empty : GridContent.Island;
        }

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

        public int DistanceBetween(Position first, Position second)
            => Math.Abs(first.X - second.X) + Math.Abs(first.Y - second.Y);
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

    public class TorpedoStrategy
    {
        private readonly Grid _grid;
        private CatBot _bot;
        private Random _random;
        private const int Range = 4;
        private const int Safety = 2;

        public TorpedoStrategy(Random random,
            Grid grid,
            CatBot bot)
        {
            _random = random;
            _bot = bot;
            _grid = grid;
        }

        public Position GetTarget()
        {
            Position target;
            do
            {
                target = new Position(_random.Next(-1 * Range, Range + 1) + _bot.Position.X,
                    _random.Next(-1 * Range, Range + 1) + _bot.Position.Y);
                Console.Error.WriteLine($"Generated target {target}");
            } while (!IsValidTarget(target));

            return target;
        }

        private bool IsValidTarget(Position target)
        {
            if (!_grid.IsPositionInBounds(target))
            {
                return false;
            }
            
            int distance = _grid.DistanceBetween(_bot.Position, target);
            return _grid.ContentsAt(target) == GridContent.Empty
                && distance <= Range
                   && distance >= Safety;
        }
    }
    
    public class GridInputToBytes
    {
        public uint[] GetInt64(string[] input)
        {
            uint[] result = new uint[input.Length];

            for (int row = 0; row < input.Length; row++)
            {
                char[] reversed = input[row].Reverse().ToArray();
                uint total = 0;
                for (int i = 0; i < reversed.Length; i++)
                {
                    if (reversed[i] == 'x')
                    {
                        total += (uint)Math.Pow(2, i);
                    }
                }

                result[row] = total;
            }

            return result;
        }
    }
    
    public class GridBitRotator
    {
        private readonly uint _leftMostBitInt;

        public GridBitRotator(in int bitCount)
        {
            _leftMostBitInt = (uint)Math.Pow(2, bitCount - 1);
        }

        public uint[] Rotate(uint[] ints)
        {
            uint[] result = new uint[ints.Length];
            for (int i = 0; i < ints.Length; i++)
            {
                int leftIndex = i == 0 ? ints.Length - 1 : i - 1;
                uint incomingIfRequired = (ints[leftIndex] & 1) * _leftMostBitInt;
                result[i] = (ints[i] >> 1) | incomingIfRequired;
            }

            return result;
        }
    }
}
