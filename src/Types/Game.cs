using System.Diagnostics;
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
        private static Regex _silenceRegex = new Regex(@"SILENCE", RegexOptions.Compiled);
        private static Regex _sectorLocator = new Regex(@"(SONAR|SURFACE) (?<sector>\d+)");

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
            var knownLocationTargettingStrategy = new KnownLocationsTorpedoStrategy(grid, random);
            var locatorStrategy = new BasicEnemyLocatorStrategy(grid);
            List<EnemyMove> enemyMoves = new List<EnemyMove>();
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

                var currentMove = new EnemyMove(opponentOrders);
                enemyMoves.Add(currentMove);
                
                if (currentMove.IsSilence)
                {
                    Console.Error.WriteLine($"SILENCED {opponentOrders}");
                    enemyMoves.Clear();
                }
                
                Console.Error.WriteLine($"Spotted moves: {string.Join(" ", enemyMoves.Where(m => m.IsMovement).Select(e => (char)e.Movement))}");

                if (currentMove.HasSector)
                {
                    Console.Error.WriteLine($"Enemy in sector {currentMove.Sector}");
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");s

                var timer = new Stopwatch();
                timer.Start();
                var enemyLocations = locatorStrategy.LocateEnemy(enemyMoves.ToArray());
                timer.Stop();
                Console.Error.WriteLine($"Enemy locator took {timer.ElapsedMilliseconds}ms with {enemyLocations.Count()} possibilities");
                if (enemyLocations.Count() == 1)
                {
                    Console.Error.WriteLine($"ENEMY LOCATED AT {enemyLocations.Single()}");
                }
                
                var move = moveStrategy.GetMove(grid, catBot);
                catBot.Move(grid, move);
                string torpedo;
                if (turnNumber % 4 != 0)
                {
                    torpedo = " TORPEDO";
                }
                else
                {
                    var target = knownLocationTargettingStrategy.GetTarget(catBot.Position, enemyLocations) ?? torpedoStrategy.GetTarget();
                    torpedo = $"|TORPEDO {target.X} {target.Y}";
                }

                // Console.Error.WriteLine($"Torpedo is {torpedo}");
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

    public class OverlayGrid
    {
        private readonly uint[] _overlayGrid;
        private readonly uint[] _offsetGrid;
        public Position CurrentOffset { get; private set; }
        public bool CanMoveDown => (_offsetGrid[_offsetGrid.Length - 1] & ~(uint) 0) == 0;
        public bool CanMoveRight => _offsetGrid.All(row => (row & 1) == 0);

        public OverlayGrid(int width, int height, List<Position> positions)
        {
            _overlayGrid = GenerateGridBinary(width, height, positions);
            _offsetGrid = new uint[_overlayGrid.Length];
            _overlayGrid.CopyTo(_offsetGrid, 0);
            CurrentOffset = new Position(0,0);
        }

        private uint[] GenerateGridBinary(int width, int height, List<Position> positions)
        {
            uint[] myGrid = new uint[height];
            foreach (var position in positions)
            {
                try
                {
                    myGrid[position.Y] = myGrid[position.Y] | (uint) Math.Pow(2, width - 1 - position.X);
                }
                catch (Exception)
                {   
                    Console.Error.WriteLine($"Tried to access position index {position.Y} but only have {height} rows");
                    foreach (var p in positions)
                    {
                        Console.Error.WriteLine($"{p}");
                    }
                    throw;
                }
            }
            var grid = string.Join("\n", myGrid.Select(g => Convert.ToString(g, 2)));
            return myGrid;
        }

        public uint[] Mask(Grid grid)
        {
            if (grid.GridBinary.Length != _offsetGrid.Length)
            {
                throw new Exception("Grids are of different sizes");
            }
            
            uint[] masked = new uint[_offsetGrid.Length];
            for (int i=0;i<_offsetGrid.Length;i++)
            {
                masked[i] = grid.GridBinary[i] & _offsetGrid[i];
            }

            return masked;
        }
        
        public void ShiftRight()
        {
            CurrentOffset = new Position(CurrentOffset.X + 1, CurrentOffset.Y);
            for (int i = 0; i < _offsetGrid.Length; i++)
            {
                _offsetGrid[i] = _offsetGrid[i] >> 1;
            }
        }

        public void ShiftToOriginalX()
        {
            int offset = CurrentOffset.X;
            CurrentOffset = new Position(0, CurrentOffset.Y);
            for (int i = 0; i < _offsetGrid.Length; i++)
            {
                _offsetGrid[i] = _offsetGrid[i] << offset;
            }
        }
        
        public void ShiftDown()
        {
            CurrentOffset = new Position(CurrentOffset.X, CurrentOffset.Y + 1);
            uint temp = _offsetGrid[_offsetGrid.Length - 1];
            for (int i = _offsetGrid.Length - 1; i > 0; i--)
            {
                _offsetGrid[i] = _offsetGrid[i - 1];
            }

            _offsetGrid[0] = temp;
        }
    }

    public class BasicEnemyLocatorStrategy
    {
        private readonly Grid _grid;

        public BasicEnemyLocatorStrategy(Grid grid)
        {
            _grid = grid;
        }

        private (int N, int S, int E, int W) GetMoveCounts(IEnumerable<Direction> moves)
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

            return (northCount, southCount, eastCount, westCount);
        }

        public IEnumerable<Position> LocateEnemy(IEnumerable<EnemyMove> moves)
        {
            var directions = moves
                .Where(m => m.IsMovement)
                .Select(m => m.Movement);
            var positions = FindPositions(directions);
            var search = new OverlayGrid(_grid.Width, _grid.Height, positions);

            List<Position> offsetMatches = new List<Position>();

            do
            {
                var mask = search.Mask(_grid);

                if (mask.All(m => m == 0))
                {
                    offsetMatches.Add(search.CurrentOffset);
                    // Console.Error.WriteLine($"Found a place the enemy could be hiding offset {offset}");
                }
                
                if (!search.CanMoveRight && !search.CanMoveDown)
                {
                    break;
                }

                if (search.CanMoveRight)
                {
                    search.ShiftRight();
                    continue;
                }

                search.ShiftToOriginalX();
                search.ShiftDown();

            } while (true);

            Position lastPosition = positions[positions.Count - 1];
            return offsetMatches.Select(m => new Position(m.X + lastPosition.X, m.Y + lastPosition.Y));
        }

        private List<Position> FindPositions(IEnumerable<Direction> moves)
        {
            var counts = GetMoveCounts(moves);
            List<Position> positions = new List<Position>();
            Position currentPosition = new Position(0, 0);
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

            int xMin = positions.Min(p => p.X);
            int yMin = positions.Min(p => p.Y);

            int xOffset = xMin < 0 ? Math.Abs(xMin) : 0;
            int yOffset = yMin < 0 ? Math.Abs(yMin) : 0;
            
            return positions.Select(p => new Position(p.X + xOffset, p.Y + yOffset)).ToList();
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

        public Grid(GridInputToBytes gridInputToBytes, string[] input)
        {
            if (input.GroupBy(i => i.Length).Count() > 1)
            {
                throw new ArgumentException("Input is not of equal lengths");
            }

            GridBinary = gridInputToBytes.GetInt64(input);
            string gridString = string.Join("\n", GridBinary.Select(g => Convert.ToString(g, 2)));
            Width = input[0].Length;
            Height = input.Length;
            
            MaskUnusedBits(GridBinary);
        }
        
        private void MaskUnusedBits(uint[] grid)
        {
            uint mask = ~(uint) Enumerable.Range(0, Width).Sum(i => (uint) Math.Pow(2, i));

            for (int i = 0; i < GridBinary.Length; i++)
            {
                grid[i] = grid[i] | mask;
            }
        }

        public GridContent ContentsAt(Position position)
        {
            if (!IsPositionInBounds(position))
            {
                throw new InvalidOperationException("Position is not in bounds");
            }
            
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

    public class KnownLocationsTorpedoStrategy
    {
        private readonly Grid _grid;
        private readonly Random _random;

        public KnownLocationsTorpedoStrategy(Grid grid, Random random)
        {
            _random = random;
            _grid = grid;
        }
        
        public Position? GetTarget(Position myLocation, IEnumerable<Position> locations)
        {
            var inRange = locations.Where(l => _grid.DistanceBetween(myLocation, l) <= 4).ToList();
            if (!inRange.Any() || inRange.Count > 5)
            {
                return null;
            }

            return inRange[_random.Next(0, inRange.Count)];
        }
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
    
    public struct EnemyMove
    {
        private static readonly Regex _moveRegex = new Regex("MOVE (?<move>[NSEW])", RegexOptions.Compiled);
        private static readonly Regex SectorRegex = new Regex(@"(SONAR|SURFACE) (?<sector>\d)", RegexOptions.Compiled);
        private static readonly Regex SilenceRegex = new Regex(@"SILENCE", RegexOptions.Compiled);
        private readonly Direction? _direction;
        private readonly Sector? _sector;
        
        public EnemyMove(string input)
        {
            var move = _moveRegex.Match(input);
            _direction = move.Success ? (Direction?)move.Groups["move"].Value[0] : null;

            var sector = SectorRegex.Match(input);
            _sector = sector.Success ? (Sector?) new Sector(int.Parse(sector.Groups["sector"].Value)) : null;

            IsSilence = SilenceRegex.IsMatch(input);
        }
    
        public bool IsMovement => _direction.HasValue;
        public Direction Movement => _direction.Value;
        public bool HasSector => _sector.HasValue;
        public Sector Sector => _sector.Value;
        public bool IsSilence { get; }
    }

    public struct Sector
    {
        public Sector(int sector)
        {
            Number = sector;
        }
        
        public int Number { get; }
    }
}
