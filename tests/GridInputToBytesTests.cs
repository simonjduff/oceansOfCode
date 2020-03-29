using Types;
using Xunit;

namespace tests
{
    public class GridInputToBytesTests
    {
        [Theory]    
        [InlineData(0, "...")]
        [InlineData(7, "xxx")]
        [InlineData(5, "x.x")]
        public void Can_convert_input_to_bytes(uint expected, params string[] input)
        {
            GridInputToBytes gridInputToBytes = new GridInputToBytes();

            uint[] actual = gridInputToBytes.GetInt64(input);
            
            Assert.Single(actual);
            Assert.Equal(expected, actual[0]);
        }

        [Fact]
        public void Can_convert_multiple_rows()
        {
            GridInputToBytes gridInputToBytes = new GridInputToBytes();

            uint[] actual = gridInputToBytes.GetInt64(new[]{"x.x","xx"});
            uint[] expected = new uint[] {5, 3};
            
            Assert.Equal(expected, actual);
        }
    }
}