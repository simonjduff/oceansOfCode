using System;
using System.Linq;
using Types;
using Xunit;

namespace tests
{
    public class CatBotTests
    {
        [Fact]
        public void Selects_start_position()
        {
            // Given a grid
            var input = new string[]
            {
                ".....",
                "xxxxx",
                "x.x.x",
                ".x.x."
            };

            Grid grid = new Grid(new GridInputToBytes(), input);

            // When I set the start position
            var position = new Position(2, 1);
            CatBot bot = new CatBot(position);

            GridContent content = grid.ContentsAt(bot.Position);

            // Then the position is set
            Assert.Equal(position, bot.Position);
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
                "x.x.x",
                "...x."
            };
            
            Grid grid = new Grid(new GridInputToBytes(), input);
            CatBot bot = new CatBot(new Position(startX, startY));

            // When I check move validity
            bool isValid = bot.IsValidMove(grid, (MoveDirection)move);
            
            // Then the player position moves
            Assert.Equal(expected, isValid);
        }
        
        [Fact]
        public void Player_can_move()
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
            
            // And a start position
            CatBot bot = new CatBot(new Position(1, 2));

            // When I move
            bot.Move(grid, MoveDirection.South);

            // Then the player has moved
            Assert.Equal(new Position(1, 3), bot.Position);
        }
        
        [Fact]
        public void CatBot_has_history()
        {
            CatBot CatBot = new CatBot(new Position(0,0));

            CatBot.Move(new Position(0, 1));

            Assert.Equal(2, CatBot.History.Count());
            Assert.Contains(new Position(0, 0), CatBot.History);
            Assert.Contains(new Position(0, 1), CatBot.History);
        }

        [Fact]
        public void CatBot_cannot_backtrack()
        {
            CatBot CatBot = new CatBot(new Position(0,0));

            CatBot.Move(new Position(0, 1));

            Assert.Throws<InvalidOperationException>(() => CatBot.Move(new Position(0, 0)));
        }
    }
}