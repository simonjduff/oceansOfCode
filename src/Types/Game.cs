using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

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
        public static readonly Regex MoveRegex = new Regex("MOVE (?<move>[NSEW])", RegexOptions.Compiled);
        public static readonly Regex SectorRegex = new Regex(@"(SONAR|SURFACE) (?<sector>\d)", RegexOptions.Compiled);
        public static readonly Regex SilenceRegex = new Regex(@"SILENCE", RegexOptions.Compiled);
        public static uint LeftColumnMask =   0b_00000_11111_11111;
        public static uint MiddleColumnMask = 0b_11111_00000_11111;
        public static uint RightColumnMask =  0b_11111_11111_00000;
        public static uint UnusedBitsMask = ~(uint) Enumerable.Range(0, 15).Sum(i => (uint) Math.Pow(2, i));
        public static Random Random = new Random();
        
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
            
            // var moveStrategy = new RandomMoveStrategy(random);
            var moveStrategy = new GreatestOptionsMoveStrategy(6);
            var torpedoStrategy = new TorpedoStrategy(Random, grid, catBot);
            var knownLocationTargettingStrategy = new KnownLocationsTorpedoStrategy(grid, Random);
            var locatorStrategy = new BasicEnemyLocatorStrategy(grid);
            List<EnemyMove> enemyMoves = new List<EnemyMove>();
            // game loop
            while (true)
            {
                Stopwatch globalTimer = new Stopwatch();

                try
                {
                    inputs = Console.ReadLine().Split(' ');
                    globalTimer.Start();
                    int x = int.Parse(inputs[0]);
                    int y = int.Parse(inputs[1]);
                    int myLife = int.Parse(inputs[2]);
                    int oppLife = int.Parse(inputs[3]);
                    int torpedoCooldown = int.Parse(inputs[4]);
                    int sonarCooldown = int.Parse(inputs[5]);
                    int silenceCooldown = int.Parse(inputs[6]);
                    int mineCooldown = int.Parse(inputs[7]);
                    Console.Error.WriteLine($"Reading data {globalTimer.ElapsedMilliseconds}");
                    string sonarResult = Console.ReadLine();
                    string opponentOrders = Console.ReadLine();

                    Console.Error.WriteLine($"Starting turn at {globalTimer.ElapsedMilliseconds}");
                    
                    var currentMove = new EnemyMove(opponentOrders);
                    Console.Error.WriteLine($"Parsed move at {globalTimer.ElapsedMilliseconds}");
                    enemyMoves.Add(currentMove);
                    if (currentMove.IsSilence)
                    {
                        Console.Error.WriteLine($"SILENCED {opponentOrders}");
                        enemyMoves.Clear();
                    }
                
                    Console.Error.WriteLine($"Spotted moves: {string.Join(" ", enemyMoves.Where(m => m.IsMovement).Select(e => (char)e.Movement))} at {globalTimer.ElapsedMilliseconds}");

                    if (currentMove.HasSector)
                    {
                        Console.Error.WriteLine($"Enemy in sector {currentMove.Sector}");
                    }

                    // Write an action using Console.WriteLine()
                    // To debug: Console.Error.WriteLine("Debug messages...");s

                    var timer = new Stopwatch();
                    timer.Start();
                    CancellationTokenSource locationCancellation = new CancellationTokenSource(10);
                    var enemyLocations = locatorStrategy.LocateEnemy(locationCancellation.Token, enemyMoves.ToArray());
                    timer.Stop();
                    Console.Error.WriteLine($"Enemy locator took {timer.ElapsedMilliseconds}ms with {enemyLocations.Count()} possibilities at {globalTimer.ElapsedMilliseconds}");
                    if (enemyLocations.Count() == 1)
                    {
                        Console.Error.WriteLine($"ENEMY LOCATED AT {enemyLocations.Single()}");
                    }

                    CancellationTokenSource cancellation = new CancellationTokenSource(5);
                    Console.Error.WriteLine($"Starting movement at {globalTimer.ElapsedMilliseconds}");
                    var move = moveStrategy.GetMove(grid, catBot, cancellation.Token);
                    List<string> orders = new List<string>();
                    if (silenceCooldown == 0 && move != MoveDirection.Surface)
                    {
                        orders.Add(move.ToSilence());
                        catBot.Move(grid, move);
                        move = moveStrategy.GetMove(grid, catBot, cancellation.Token);
                    }
                    Console.Error.WriteLine($"Finishing movement at {globalTimer.ElapsedMilliseconds}");
                    catBot.Move(grid, move);
                    string moveString = move.ToMove();
                    Console.Error.WriteLine($"TORP {torpedoCooldown}");
                    if (torpedoCooldown > 0)
                    {
                        moveString = $"{moveString} TORPEDO";
                    }
                    else
                    {
                        var target = knownLocationTargettingStrategy.GetTarget(catBot.Position, enemyLocations);
                        if (target != null)
                        {
                            orders.Add($"TORPEDO {target.Value.X} {target.Value.Y}");
                        }
                        else if (silenceCooldown > 0)
                        {
                            moveString = $"{moveString} SILENCE";
                        }

                        // var target = knownLocationTargettingStrategy.GetTarget(catBot.Position, enemyLocations) ??
                        //              torpedoStrategy.GetTarget();
                        // torpedo = $"|TORPEDO {target.X} {target.Y}";
                    }

                    // Console.Error.WriteLine($"Torpedo is {torpedo}");
                    orders.Add(moveString);
                    Console.WriteLine(string.Join("|", orders));
                }
                finally
                {
                    Console.Error.WriteLine($"TIMER {globalTimer.ElapsedMilliseconds}");
                }
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

        public static string ToSilence(this MoveDirection direction, int distance = 1)
            => $"SILENCE {(char) direction} {distance}";

        public static MoveDirection ToMove(this Direction direction)
        {
            char dir = (char) direction;
            return (MoveDirection) dir;
        }
    }

    public interface IMoveStrategy
    {
        MoveDirection GetMove(Grid grid, CatBot bot, CancellationToken cancellation);
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
        public string OffsetString => string.Join("\n", _offsetGrid.Select(m => Convert.ToString(m, 2)));

        public OverlayGrid(int width, int height, List<SectorConstrainedPosition> positions)
        {
            _overlayGrid = GenerateGridBinary(width, height, positions);
            _offsetGrid = new uint[_overlayGrid.Length];
            _overlayGrid.CopyTo(_offsetGrid, 0);
            CurrentOffset = new Position(0,0);
        }
        

        private uint[] GenerateGridBinary(int width, int height, List<SectorConstrainedPosition> positions)
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

        public uint[] Mask(uint[] grid)
        {
            if (grid.Length != _offsetGrid.Length)
            {
                throw new Exception("Grids are of different sizes");
            }
            
            uint[] masked = new uint[_offsetGrid.Length];
            for (int i=0;i<_offsetGrid.Length;i++)
            {
                masked[i] = grid[i] & _offsetGrid[i];
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
    
    public struct SectorConstrainedPosition
    {
        public SectorConstrainedPosition(Position position, Sector? sector) : this(position.X, position.Y, sector)
        {
        }

        public SectorConstrainedPosition(int x, int y, Sector? sector)
        {
            X = x;
            Y = y;
            SectorConstraint = sector;
        }
            
        public int X { get; }
        public int Y { get; }
        public bool IsSectorConstrained => SectorConstraint.HasValue;
        public Sector? SectorConstraint { get; }
    }

    public class BasicEnemyLocatorStrategy
    {
        private readonly Grid _grid;

        public BasicEnemyLocatorStrategy(Grid grid)
        {
            _grid = grid;
        }

        public IEnumerable<Position> LocateEnemy(CancellationToken cancellation, IEnumerable<EnemyMove> moves)
        {
            if (moves.Count() < 3)
            {
                return Enumerable.Empty<Position>();
            }
            
            var positions = FindPositions(moves);
            var search = new OverlayGrid(_grid.Width, _grid.Height, positions);
            Dictionary<Sector, OverlayGrid> sectorGrids =
                positions.Where(p => p.IsSectorConstrained)
                    .GroupBy(p => p.SectorConstraint.Value)
                    .ToDictionary(p => p.Key,
                        g => new OverlayGrid(_grid.Width, _grid.Height, g.ToList()));
            
            
            List<Position> offsetMatches = new List<Position>();

            do
            {
                if (cancellation.IsCancellationRequested)
                {
                    Console.Error.WriteLine("CANCELLING TARGETTING");
                    break;
                }
                
                var mask = search.Mask(_grid.GridBinary);

                if (PathDoesNotIntersectWithIslands(mask)
                    && NoSectorCollisions(sectorGrids))
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
                    foreach (var sectorGrid in sectorGrids.Values)
                    {
                        sectorGrid.ShiftRight();
                    }
                    continue;
                }

                search.ShiftToOriginalX();
                search.ShiftDown();
                foreach (var sectorGrid in sectorGrids.Values)
                {
                    sectorGrid.ShiftToOriginalX();
                    sectorGrid.ShiftDown();
                }

            } while (true);

            SectorConstrainedPosition lastPosition = positions[positions.Count - 1];
            return offsetMatches.Select(m => new Position(m.X + lastPosition.X, m.Y + lastPosition.Y));
        }

        private bool NoSectorCollisions(Dictionary<Sector, OverlayGrid> sectorGrids)
        {
            foreach (var sector in sectorGrids.Keys)
            {
                var mask = SectorMasks.MaskForSector(sector);
                string maskString = string.Join("\n", mask.Select(m => Convert.ToString(m, 2)));
                string sectorString = sectorGrids[sector].OffsetString;
                if (sectorGrids[sector].Mask(mask).Any(m => m != 0))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool PathDoesNotIntersectWithIslands(uint[] mask)
        {
            return mask.All(m => m == 0);
        }

        private List<SectorConstrainedPosition> FindPositions(IEnumerable<EnemyMove> moves)
        {
            List<SectorConstrainedPosition> positions = new List<SectorConstrainedPosition>();
            SectorConstrainedPosition currentPosition = new SectorConstrainedPosition(0, 0, null);
            positions.Add(currentPosition);
            foreach (var move in moves)
            {
                if (!move.IsMovement && move.HasSector)
                {
                    currentPosition = new SectorConstrainedPosition(currentPosition.X, currentPosition.Y, move.Sector);
                    positions[positions.Count - 1] = currentPosition;
                    continue;
                }
                SectorConstrainedPosition newPosition;

                if (move.IsMovement)
                {
                    switch (move.Movement)
                    {
                        case Direction.East:
                            newPosition = new SectorConstrainedPosition(currentPosition.X + 1,
                                currentPosition.Y,
                                move.Sector);
                            break;
                        case Direction.North:
                            newPosition = new SectorConstrainedPosition(currentPosition.X,
                                currentPosition.Y - 1,
                                move.Sector);
                            break;
                        case Direction.South:
                            newPosition = new SectorConstrainedPosition(currentPosition.X,
                                currentPosition.Y + 1,
                                move.Sector);
                            break;
                        case Direction.West:
                            newPosition = new SectorConstrainedPosition(currentPosition.X - 1,
                                currentPosition.Y,
                                move.Sector);
                            break;
                        default:
                            throw new InvalidOperationException($"Invalid move {move}");
                    }
                    
                    positions.Add(newPosition);
                    currentPosition = newPosition;
                }
            }

            int xMin = positions.Min(p => p.X);
            int yMin = positions.Min(p => p.Y);

            int xOffset = xMin < 0 ? Math.Abs(xMin) : 0;
            int yOffset = yMin < 0 ? Math.Abs(yMin) : 0;
            
            return positions.Select(p => 
                new SectorConstrainedPosition(p.X + xOffset, p.Y + yOffset, p.SectorConstraint)).ToList();
        }
    }

    public class GreatestOptionsMoveStrategy : IMoveStrategy
    {
        private readonly int _searchDepth;

        public GreatestOptionsMoveStrategy(int searchDepth)
        {
            _searchDepth = searchDepth;
        }
        
        public MoveDirection GetMove(Grid grid, CatBot bot, CancellationToken cancellation)
        {
            TreeNode root = new TreeNode(grid, bot, _searchDepth, cancellation);
            var result = root.Traverse(new[] {bot.Position}, null);
            return result.Direction;
        }

        private class TreeNode
        {
            private int _depth;
            private readonly Grid _grid;
            private CatBot _bot;
            private CancellationToken _cancellation;

            public TreeNode(Grid grid,
                CatBot bot,
                int maxDepth,
                CancellationToken cancellation)
            {
                _cancellation = cancellation;
                _bot = bot;
                _grid = grid;
                _depth = maxDepth;
            }

            public (MoveDirection Direction, int Depth) Traverse(Position[] path, MoveDirection? direction)
            {
                if (_cancellation.IsCancellationRequested)
                {
                    Console.Error.WriteLine("CANCELLING MOVEMENT");
                    return (direction ?? MoveDirection.Surface, path.Length);
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

                return best[Player.Random.Next(0, best.Length)];
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
        
        public MoveDirection GetMove(Grid grid, CatBot bot, CancellationToken cancellation)
        {
            MoveDirection move;
            do
            {
                if (cancellation.IsCancellationRequested)
                {
                    return MoveDirection.Surface;
                }
                
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
    
    public static class SectorMasks
    {
        private static uint[][] _masks;

        static SectorMasks()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            GenerateSectorMasks();
            timer.Stop();
            Console.Error.WriteLine($"Sector masks took {timer.ElapsedMilliseconds}");
        }

        public static uint[] MaskForSector(Sector sector)
            => _masks[sector.Number -1];
        
        private static void GenerateSectorMasks()
        {
            _masks = new uint[9][];
            for (int i = 0; i < _masks.Length; i++)
            {
                int rowGroup = i / 3;
                uint mask;
                switch ((i + 1) % 3)
                {
                    case 1:
                        mask = Player.LeftColumnMask;
                        break;
                    case 2:
                        mask = Player.MiddleColumnMask;
                        break;
                    case 0:
                        mask = Player.RightColumnMask;
                        break;
                    default:
                        throw new Exception($"Tried to get a mask for sector {i}");
                }
                
                _masks[i] = new uint[15]; // Assuming the grid size is always 15...
                for (int row = 0; row < _masks[i].Length; row++)
                {
                    if (row >= rowGroup * 5 && row < rowGroup * 5 + 5)
                    {
                        _masks[i][row] = (mask & uint.MaxValue) | Player.UnusedBitsMask;
                        continue;
                    }
                    
                    _masks[i][row] = uint.MaxValue;
                }
            }
        }
    }

    public class Grid
    {
        private readonly uint _unusedBitsMask;
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
            _unusedBitsMask = ~(uint) Enumerable.Range(0, Width).Sum(i => (uint) Math.Pow(2, i));
            
            MaskUnusedBits(GridBinary);
        }

        private void MaskUnusedBits(uint[] grid)
        {
            uint mask = _unusedBitsMask;

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
        public Position FindLocation(Grid grid)
        {
            Position start;
            GridContent startContent;
            do
            {
                start = new Position(Player.Random.Next(0, grid.Width), Player.Random.Next(0, grid.Height));
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
            var inRange = locations.Where(l => 
                _grid.DistanceBetween(myLocation, l) <= 4
                && (Math.Abs(l.X - myLocation.X) > 2 || Math.Abs(l.Y - myLocation.Y) > 2) 
                ).ToList();
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

    public struct EnemyMove
    {
        private readonly Direction? _direction;
        private readonly Sector? _sector;
        
        public EnemyMove(string input)
        {
            var move = Player.MoveRegex.Match(input);
            _direction = move.Success ? (Direction?)move.Groups["move"].Value[0] : null;

            var sector = Player.SectorRegex.Match(input);
            _sector = sector.Success ? (Sector?) new Sector(int.Parse(sector.Groups["sector"].Value)) : null;

            IsSilence = Player.SilenceRegex.IsMatch(input);
        }
    
        public bool IsMovement => _direction.HasValue;
        public Direction Movement => _direction.Value;
        public bool HasSector => _sector.HasValue;
        public Sector? Sector => _sector;
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
