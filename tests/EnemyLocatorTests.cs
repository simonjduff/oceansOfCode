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
        public void Can_find_enemy_by_torpedo_targets()
        {
            
        }
    }
}