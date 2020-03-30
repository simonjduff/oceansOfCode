using System.Collections.Generic;
using System.Linq;
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
                ".....",
                "xxxxx",
                "x.x.x",
                "...x.",
                "....."
            };
            
            Grid grid = new Grid(new GridInputToBytes(), input);
            
            // And an enemy move list    
            var moves = new Direction[]
            {
                Direction.North,
                Direction.East,
                Direction.East,
                Direction.South,
                Direction.East
            };
            
            // When I find the enemy
            var locator = new BasicEnemyLocatorStrategy(grid);
            IEnumerable<Position> possibleLocations = locator.LocateEnemy(moves);
            
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
            var moves = Enumerable.Range(0, 100).SelectMany(i => new[] {Direction.North, Direction.South});
            
            // When I find the enemy
            var locator = new BasicEnemyLocatorStrategy(grid);
            IEnumerable<Position> possibleLocations = locator.LocateEnemy(moves);
        }

        [Fact]
        public void Zero_possibilities_bug()
        {
            // Given a grid
            var input = new string[]
            {
                "...............",
                "...............",
                "...............",
                ".........xx...x",
                ".........xx...x",
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
            
            Grid grid = new Grid(new GridInputToBytes(), input);

            var moves = new[]
            {
                'N', 'N', 'N', 'N', 'N', 'N', 'N', 'N', 'N', 'N', 'N', 
                'N', 'E', 'E', 'E', 'S', 'W', 'S', 'S', 'E', 'E', 'S', 
                'W', 'W', 'S', 'E', 'E', 'S', 'E', 'S', 'S', 'N', 'E', 
                'N', 'N', 'E', 'N', 'W', 'N', 'E', 'E', 'E'
            }.Select(m => (Direction)m);
            
            var locator = new BasicEnemyLocatorStrategy(grid);
            IEnumerable<Position> possibleLocations = locator.LocateEnemy(moves);
            
            // Then there is more than one possibility
            Assert.NotEmpty(possibleLocations);
        }
    }
}