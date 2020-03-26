using System;
using Xunit;

namespace tests
{
    public class StartLocatorTests
    {
        [Fact]
        public void Start_location_is_empty()
        {
            // Given a grid
            
            var input = new string[]
            {
                ".....",
                "xxxxx",
                "x.x.x.",
                ".x.x."
            };
            
            Grid grid = new Grid(input);
            
            // And a start locator
            StartLocator startLocator = new StartLocator();
            
            // When I request a start location
            (int x, int y) start = startLocator.FindLocation(grid);
            
            // Then the start location is empty
            Assert.Equal(GridContent.Empty, grid.ContentsAt(start.x, start.y));
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