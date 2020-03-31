using System.Text.RegularExpressions;
using Types;
using Xunit;

namespace tests
{
    public class EnemyMoveTests
    {
        [Theory]
        [InlineData("MOVE S", Direction.South)]
        [InlineData("MOVE N", Direction.North)]
        [InlineData("MOVE E", Direction.East)]
        [InlineData("MOVE W", Direction.West)]
        [InlineData("MOVE S | SURFACE 4", Direction.South)]
        [InlineData("SURFACE 3 | MOVE S", Direction.South)]
        [InlineData("SURFACE 3 | MOVE S | TORPEDO", Direction.South)]
        public void EnemyMove_can_parse_movement(string input, Direction expected)
        {
            EnemyMove move = new EnemyMove(input);

            Assert.True(move.IsMovement);
            Assert.Equal(expected, move.Movement);
        }
        
        [Theory]
        [InlineData("SURFACE 3", 3)]
        [InlineData("SONAR 2", 2)]
        [InlineData("MOVE S | SURFACE 4", 4)]
        [InlineData("SURFACE 3 | MOVE S", 3)]
        [InlineData("SURFACE 3 | MOVE S | TORPEDO", 3)]
        public void EnemyMove_can_parse_sector(string input, int expected)
        {
            EnemyMove move = new EnemyMove(input);

            Assert.True(move.HasSector);
            Assert.Equal(expected, move.Sector.Value.Number);
        }

        [Theory]
        [InlineData("SILENCE", true)]
        [InlineData("SILENCE | SONAR 1", true)]
        [InlineData("SONAR 2", false)]
        public void EnemyMove_can_parse_silence(string input, bool expected)
        {
            EnemyMove move = new EnemyMove(input);
            
            Assert.Equal(expected, move.IsSilence);
        }
    }    
}