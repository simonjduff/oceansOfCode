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
            IEnumerable<Position> possibleLocations = locator.LocateEnemy(moves.Select(m => new EnemyMove(m)));
            
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
            IEnumerable<Position> possibleLocations = locator.LocateEnemy(moves);
        }

        // [Fact]
        // public void Sector_matching()
        // {
        //     // Given a grid
        //     var input = new string[]
        //     {
        //         "...............",
        //         "...............",
        //         "...............",
        //         ".........xx...x",
        //         ".........xx...x",
        //         ".........xxx...",
        //         ".......xxxxx...",
        //         ".xx....xxxxx...",
        //         ".xx....xxxxx...",
        //         "...............",
        //         "......xx...xxx.",
        //         "......xx...xxxx",
        //         "...........xxxx",
        //         "............xx.",
        //         "............xx."
        //     };
        //     
        //     Grid grid = new Grid(new GridInputToBytes(), input);
        //
        //     var moves = new[]
        //     {
        //         "MOVE S", "MOVE S", "SURFACE 3 | MOVE S", "MOVE S",
        //         "MOVE W","MOVE W", "MOVE W",  
        //     }.Select(m => new EnemyMove(m));
        //     
        //     var locator = new SectorAwareEnemyLocatorStrategy(grid);
        //     IEnumerable<Position> possibleLocations = locator.LocateEnemy(moves);
        //     
        //     // Then there is one possibility
        //     Assert.Single(possibleLocations);
        //         Assert.Equal(new Position(11, 4), possibleLocations.Single());
        // }
    }
}