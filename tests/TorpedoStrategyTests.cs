using System;
using System.Diagnostics;
using Types;
using Xunit;

namespace tests
{
    public class TorpedoStrategyTests
    {
        [Theory]
        [InlineData(0,0)]
        [InlineData(1,0)]
        [InlineData(2,0)]
        [InlineData(3,0)]
        [InlineData(4,0)]
        [InlineData(0,2)]
        [InlineData(1,2)]
        [InlineData(0,3)]
        [InlineData(1,3)]
        [InlineData(2,3)]
        [InlineData(4,3)]
        [InlineData(0,4)]
        [InlineData(1,4)]
        [InlineData(2,4)]
        [InlineData(3,4)]
        [InlineData(4,4)]
        public void Targets_valid_square(int x, int y)
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
            
            // And a bot
            CatBot bot = new CatBot(new Position(x, y));

            // When I generate a firing solution
            var torpoedoStrategy = new TorpedoStrategy(new Random(), grid, bot);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Position target = torpoedoStrategy.GetTarget();
            timer.Stop();
            
            // Then the calculation was quick
            Assert.True(timer.ElapsedMilliseconds < 10);
            
            // And the target is inside the grid
            Assert.True(grid.IsPositionInBounds(target), "In bounds");
            
            // And the target is empty
            Assert.Equal(GridContent.Empty, grid.ContentsAt(target));
            
            // And in targetting range
            int distance = grid.DistanceBetween(bot.Position, target);
            Assert.True(distance <= 4, "Not out of range");
            
            // And not my square
            Assert.NotEqual(bot.Position, target);
            
            // And not next to me
            Assert.True(distance >= 2, "Not next to me");
        }
    }
}