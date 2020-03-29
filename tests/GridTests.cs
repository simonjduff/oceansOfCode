using System;
using System.Linq;
using Types;
using Xunit;

namespace tests
{
    public class GridTests
    {
        [Theory]
        [InlineData(0, 0, GridContent.Empty)]
        [InlineData(1, 0, GridContent.Empty)]
        [InlineData(2, 0, GridContent.Empty)]
        [InlineData(3, 0, GridContent.Empty)]
        [InlineData(4, 0, GridContent.Empty)]
        [InlineData(0, 1, GridContent.Island)]
        [InlineData(1, 1, GridContent.Island)]
        [InlineData(2, 1, GridContent.Island)]
        [InlineData(3, 1, GridContent.Island)]
        [InlineData(4, 1, GridContent.Island)]
        [InlineData(1, 2, GridContent.Empty)]
        [InlineData(2, 2, GridContent.Island)]
        [InlineData(3, 2, GridContent.Empty)]
        [InlineData(4, 2, GridContent.Island)]
        public void Can_identify_contents(int x, int y, GridContent expected)
        {
            // Given a grid
            var input = new string[]
            {
                ".....",
                "xxxxx",
                "x.x.x",
                ".x.x."
            };
            
            Grid grid = new Grid(new GridInputToBytes(),  input);
            
            // When I ask for the grid contents
            GridContent content = grid.ContentsAt(new Position(x, y));
            
            // Then the contents are as expected
            Assert.Equal(expected, content);
        }

        [Theory]
        [InlineData(-1, 0, false)]
        [InlineData(0, -1, false)]
        [InlineData(-1, -1, false)]
        [InlineData(5, 0, false)]
        [InlineData(4, 0, true)]
        [InlineData(0, 5, false)]
        [InlineData(0, 4, true)]
        [InlineData(4, 4, true)]
        public void Check_if_position_in_bounds(int x, int y, bool expected)
        {
            // Given a grid
            var input = new string[]
            {
                ".....",
                "xxxxx",
                "x.x.x",
                "...x.",
                "....."
            };
            
            Grid grid = new Grid(new GridInputToBytes(), input);
            
            // When I check a position
            bool valid = grid.IsPositionInBounds(new Position(x, y));
            
            // Then it matches
            Assert.Equal(expected, valid);
        }
        
        
        [Fact]
        public void Can_get_neighbours()
        {
            // Given a grid
            var input = new string[]
            {
                ".....",
                "xxxxx",
                "x.x.x",
                "...x.",
                "....."
            };
            
            Grid grid = new Grid(new GridInputToBytes(), input);
            
            // When I ask for neighbours of a cell
            Position[] neighbours = grid.Neighbours(new Position(0, 0)).ToArray();
            
            // Then the neighbours are returned
            Assert.Equal(2, neighbours.Length);
            Assert.Contains(new Position(0, 1), neighbours);
            Assert.Contains(new Position(1, 0), neighbours);
        }

        [Theory]
        [InlineData(0,0,3,3,6)]
        [InlineData(0,0,3,2,5)]
        [InlineData(0,0,0,1,1)]
        [InlineData(3,3,0,0,6)]
        public void Distance_between_two_points(int x1, int y1, int x2, int y2, int expected)
        {
            // Given a grid
            var input = new string[]
            {
                ".....",
                "xxxxx",
                "x.x.x",
                "...x.",
                "....."
            };
            
            Grid grid = new Grid(new GridInputToBytes(), input);
            
            // When I ask for distance
            int distance = grid.DistanceBetween(new Position(x1, y1), new Position(x2, y2));
            
            // Then the distance is correct
            Assert.Equal(expected, distance);
        }

        [Theory]
        [InlineData(4,4,true, true)]
        [InlineData(9,7,true, false)]
        public void Big_grid_doesnt_fail(int x, int y, bool expectedInBounds, bool expectedEmpty)
        {
            // Given a grid
            var input = new string[]
            {
                "..........",
                "xxxxx.....",
                "x.x.x.....",
                "...x......",
                "..........",
                "..........",
                "..........",
                "......x..x"
            };
            
            Grid grid = new Grid(new GridInputToBytes(), input);
            
            // When I check a position
            var position = new Position(x, y);
            bool inBounds = grid.IsPositionInBounds(position);
            bool empty = grid.ContentsAt(position) == GridContent.Empty;
            
            
            // Then it matches
            Assert.Equal(expectedInBounds, inBounds);
            Assert.Equal(expectedEmpty, empty);
        }
    }
}