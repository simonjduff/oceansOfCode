using System;
using Types;
using Xunit;
using System.Linq;

namespace tests
{
    public class UboatTests
    {
        [Fact]
        public void Uboat_has_history()
        {
            Uboat uboat = new Uboat(new Position(0,0));

            uboat.Move(new Position(0, 1));

            Assert.Equal(2, uboat.History.Count());
            Assert.Contains(new Position(0, 0), uboat.History);
            Assert.Contains(new Position(0, 1), uboat.History);
        }

        [Fact]
        public void Uboat_cannot_backtrack()
        {
            Uboat uboat = new Uboat(new Position(0,0));

            uboat.Move(new Position(0, 1));

            Assert.Throws<InvalidOperationException>(() => uboat.Move(new Position(0, 0)));
        }
    }
}