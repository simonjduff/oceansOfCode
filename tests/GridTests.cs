using System;
using System.Linq;
using Xunit;

namespace tests
{
    public class GridTests
    {
        [Theory]
        [InlineData(0, 0, GridContent.Empty)]
        [InlineData(0, 1, GridContent.Empty)]
        [InlineData(0, 2, GridContent.Empty)]
        [InlineData(0, 3, GridContent.Empty)]
        [InlineData(0, 4, GridContent.Empty)]
        [InlineData(1, 0, GridContent.Island)]
        [InlineData(1, 1, GridContent.Island)]
        [InlineData(1, 2, GridContent.Island)]
        [InlineData(1, 3, GridContent.Island)]
        [InlineData(1, 4, GridContent.Island)]
        [InlineData(2, 0, GridContent.Island)]
        [InlineData(2, 1, GridContent.Empty)]
        [InlineData(2, 2, GridContent.Island)]
        [InlineData(2, 3, GridContent.Empty)]
        [InlineData(2, 4, GridContent.Island)]
        public void Can_identify_contents(int x, int y, GridContent expected)
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
            
            // When I ask for the grid contents
            GridContent content = grid.ContentsAt(x, y);
            
            // Then the contents are as expected
            Assert.Equal(expected, content);
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
}