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
        [InlineData(2, 1, GridContent.Island)]
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
                "x.x.x.",
                ".x.x."
            };
            
            Grid grid = new Grid(input);
            
            // When I ask for the grid contents
            GridContent content = grid.ContentsAt(new Position(x, y));
            
            // Then the contents are as expected
            Assert.Equal(expected, content);
        }

        [Theory]
        [InlineData(2, 1)]
        public void Sets_start_position(int x, int y)
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
            
            // When I set the start position
            var position = new Position(x, y);
            grid.SetPlayerPosition(position);

            GridContent content = grid.ContentsAt(position);
            
            // Then the position is set
            Assert.Equal(position, grid.PlayerPosition);
        }

        [Fact]
        public void Cant_set_player_twice()
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
            
            // When I set the start position
            var position = new Position(0, 0);
            grid.SetPlayerPosition(position);

            // Then setting the position again throws
            Assert.Throws<InvalidOperationException>(() => grid.SetPlayerPosition(new Position(0, 1)));
        }

        [Theory]
        [InlineData(0, 0, 'N', false)]
        [InlineData(0, 0, 'E', true)]
        [InlineData(0, 0, 'S', false)]
        [InlineData(0, 0, 'W', false)]
        [InlineData(1, 3, 'N', true)]
        [InlineData(1, 3, 'E', true)]
        [InlineData(1, 3, 'S', false)]
        [InlineData(1, 3, 'W', true)]
        public void Checks_if_move_valid(int startX, int startY, char move, bool expected)
        {
            // Given a grid
            var input = new string[]
            {
                ".....",
                "xxxxx",
                "x.x.x.",
                "...x."
            };
            
            Grid grid = new Grid(input);
            grid.SetPlayerPosition(new Position(startX, startY));

            // When I check move validity
            bool isValid = grid.IsValidMove(grid.PlayerPosition, (Direction)move);
            
            // Then the player position moves
            Assert.Equal(expected, isValid);
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
        public void Check_if_position_valid(int x, int y, bool expected)
        {
            // Given a grid
            var input = new string[]
            {
                ".....",
                "xxxxx",
                "x.x.x.",
                "...x.",
                "....."
            };
            
            Grid grid = new Grid(input);
            
            // When I check a position
            bool valid = grid.IsPositionValid(new Position(x, y));
            
            // Then it matches
            Assert.Equal(expected, valid);
        }

        [Fact]
        public void Player_can_move()
        {
            // Given a grid
            var input = new string[]
            {
                ".....",
                "xxxxx",
                "x.x.x.",
                "...x.",
                "....."
            };
            
            Grid grid = new Grid(input);
            
            // And a start position
            grid.SetPlayerPosition(new Position(1, 2));
            
            // When I move
            grid.MovePlayer(Direction.South);

            // Then the player has moved
            Assert.Equal(new Position(1, 3), grid.PlayerPosition);
        }
    }
}