using System;
using Types;
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
                "x.x.x",
                ".x.x."
            };
            
            Grid grid = new Grid(input);
            
            // And a start locator
            StartLocator startLocator = new StartLocator();
            
            // When I request a start location
            Position start = startLocator.FindLocation(grid);
            
            // Then the start location is empty
            Assert.Equal(GridContent.Empty, grid.ContentsAt(start));
        }
    }
}