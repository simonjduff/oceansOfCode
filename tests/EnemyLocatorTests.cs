using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Types;
using Xunit;

namespace tests
{
    public class EnemyLocatorTests
    {
        [Fact]
        public void Can_find_enemy_by_moves()
        {
            // Given a grid
            var input = new string[]
            {
                ".....xxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "x.x.xxxxxxxxxxx",
                "...x.xxxxxxxxxx",
                ".....xxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
            };
            
            Grid grid = new Grid(new GridInputToBytes(), input);
            
            // And an enemy move list    
            var moves = new string[]
            {
                "MOVE N",
                "MOVE E",
                "MOVE E",
                "MOVE S",
                "MOVE E"
            };
            
            // When I find the enemy
            var locator = new BasicEnemyLocatorStrategy(grid);
            IEnumerable<Position> possibleLocations = locator.LocateEnemy(new CancellationToken(),  moves.Select(m => new EnemyMove(m)));
            
            // Then the location is correct
            Assert.Single(possibleLocations);
            Assert.Equal(new Position(3, 4), possibleLocations.Single());
        }

        [Fact]
        public void Fix_crash_bug()
        {
            // Given a grid
            var input = new string[]
            {
                "..xxxxx.xx..xxx",
                "..xxxx..xx..xx.",
                "..xxxxx........",
                "..xxxxx..xx....",
                ".........xx....",
                ".......xx......",
                ".......xx......",
                "...xx..........",
                "...xx..........",
                "...............",
                "...............",
                "...............",
                ".........xx....",
                ".........xx....",
                "..............."
            };
            
            Grid grid = new Grid(new GridInputToBytes(), input);
            
            // And an enemy move list    
            var moves = Enumerable.Range(0, 100).SelectMany(i => new[] {
                new EnemyMove("MOVE N"), 
                new EnemyMove("MOVE S")});
            
            // When I find the enemy
            var locator = new BasicEnemyLocatorStrategy(grid);
            IEnumerable<Position> possibleLocations = locator.LocateEnemy(new CancellationToken(),  moves);
        }

        [Theory]
        [InlineData(10, 4, "MOVE S", "MOVE S", "SURFACE 3 | MOVE S", "MOVE S",
            "MOVE W","MOVE W", "MOVE W")]
        [InlineData(10, 4, "MOVE S", "MOVE S", "MOVE S | SURFACE 3", "MOVE S",
            "MOVE W","MOVE W", "MOVE W")]
        public void Sector_matching(int x, int y, params string[] input)
        {
            // Given a grid
            var gridInput = new string[]
            {
                "...............",
                "...............",
                "...............",
                ".........xx...x",
                ".........x....x",
                ".........xxx...",
                ".......xxxxx...",
                ".xx....xxxxx...",
                ".xx....xxxxx...",
                "...............",
                "......xx...xxx.",
                "......xx...xxxx",
                "...........xxxx",
                "............xx.",
                "............xx."
            };
            
            Grid grid = new Grid(new GridInputToBytes(), gridInput);
        
            var moves = input.Select(m => new EnemyMove(m));
            
            var locator = new BasicEnemyLocatorStrategy(grid);
            IEnumerable<Position> possibleLocations = locator.LocateEnemy(new CancellationToken(),  moves);
            
            // Then there is one possibility
            Assert.Single(possibleLocations);
            Assert.Equal(new Position(x, y), possibleLocations.Single());
        }
    }
}