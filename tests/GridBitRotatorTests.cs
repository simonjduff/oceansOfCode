using Types;
using Xunit;

namespace tests
{
    public class GridBitRotatorTests
    {
        [Fact]
        public void Can_rotate_bits()
        {
            // Given a list of ints
            uint[] ints = new uint[]
            {
                3, // 011
                1 // 001
            };
            
            // And a number of bits per int to consider
            int bitCount = 3;
            
            // When I rotate the bits
            GridBitRotator rotator = new GridBitRotator(bitCount);
            uint[] result = rotator.Rotate(ints);
            
            Assert.Equal((uint)5, result[0]); // 101
            Assert.Equal((uint)4, result[1]); // 100
        }

        [Fact]
        public void Bigger_test()
        {
            // Given a list of ints
            uint[] ints = new uint[]
            {
                28, // 11100
                6, // 110,
                0,
                0,
                0,
                1
            };
            
            // And a number of bits per int to consider
            int bitCount = 5;
            
            // When I rotate the bits
            GridBitRotator rotator = new GridBitRotator(bitCount);
            uint[] result = rotator.Rotate(ints);
            
            Assert.Equal((uint)30, result[0]);
            Assert.Equal((uint)3, result[1]); 
            Assert.Equal((uint)0, result[2]);
            Assert.Equal((uint)0, result[5]);
            
            result = rotator.Rotate(result);
            Assert.Equal((uint)15, result[0]);
            Assert.Equal((uint)1, result[1]);
            Assert.Equal((uint)16, result[2]);
        }
    }
}