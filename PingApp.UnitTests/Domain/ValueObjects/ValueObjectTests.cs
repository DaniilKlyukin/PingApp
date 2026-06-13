using FluentAssertions;
using PingApp.Domain.Common;

namespace PingApp.UnitTests.Domain.ValueObjects;

public class ValueObjectTests
{
    private class TestValueObject : ValueObject
    {
        public string PropA { get; }
        public int PropB { get; }

        public TestValueObject(string propA, int propB)
        {
            PropA = propA;
            PropB = propB;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return PropA;
            yield return PropB;
        }
    }

    [Fact]
    public void ObjectsWithSameValues_ShouldBeEqual()
    {
        var obj1 = new TestValueObject("test", 42);
        var obj2 = new TestValueObject("test", 42);

        obj1.Should().Be(obj2);
        (obj1 == obj2).Should().BeTrue();
        obj1.GetHashCode().Should().Be(obj2.GetHashCode());
    }

    [Fact]
    public void ObjectsWithDifferentValues_ShouldNotBeEqual()
    {
        var obj1 = new TestValueObject("test", 42);
        var obj2 = new TestValueObject("diff", 42);
        var obj3 = new TestValueObject("test", 24);

        obj1.Should().NotBe(obj2);
        (obj1 != obj2).Should().BeTrue();
        obj1.Should().NotBe(obj3);
    }

    [Fact]
    public void ComparisonWithNull_ShouldBeCorrect()
    {
        var obj = new TestValueObject("test", 42);

        (obj == null).Should().BeFalse();
        (null == obj).Should().BeFalse();
    }
}