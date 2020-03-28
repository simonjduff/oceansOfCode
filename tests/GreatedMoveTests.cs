using Types;
using Xunit;

namespace tests
{
    public class GreatedMoveTests
    {
        [Theory]
        [InlineData(1,2, MoveDirection.South)]
        [InlineData(0,0, MoveDirection.East)]
        [InlineData(1,0, MoveDirection.East)]
        [InlineData(3,0, MoveDirection.West)]
        [InlineData(4,0, MoveDirection.West)]
        [InlineData(3,2, MoveDirection.Surface)]
        public void Longest_path_selected(int x, int y, MoveDirection expected)
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
            
            Grid grid = new Grid(input);
            
            // And a bot
            CatBot bot = new CatBot(new Position(x, y));
            
            // When I get the best move
            GreatestOptionsMoveStrategy moveStrategy = new GreatestOptionsMoveStrategy(5);
            var move = moveStrategy.GetMove(grid, bot);
            
            // Then the move is correct
            Assert.Equal(expected, move);
        }
    }
}