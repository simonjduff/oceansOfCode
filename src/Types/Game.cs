using System;

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

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine($"{startLocation.x} {startLocation.y}");

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

                Console.WriteLine("MOVE N TORPEDO");
            }
        }
        
        public enum GridContent
        {
            Empty,
            Island,
            Traversed
        }

        public class Grid
        {
            private readonly GridContent[] _contents;
            public int Width { get; }
            public int Height { get; }
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

            public GridContent ContentsAt(int x, int y)
            {
                int index = x * Width + y;
                return _contents[index];
            }
        }
        
        public class StartLocator
        {
            private static readonly Random _random = new Random();
            public (int x, int y) FindLocation(Grid grid)
            {
                (int x, int y) start;
                GridContent startContent;
                do
                {
                    start = (_random.Next(0, grid.Width), _random.Next(0, grid.Height));
                    startContent = grid.ContentsAt(start.x, start.y);
                } while (startContent != GridContent.Empty);

                return start;
            }
        }
    }
}
