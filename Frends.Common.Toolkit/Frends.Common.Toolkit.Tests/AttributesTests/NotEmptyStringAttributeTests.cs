using System.Linq;
using Frends.Common.Toolkit.Attributes;

namespace Frends.Common.Toolkit.Tests.AttributesTests;

public class NotEmptyStringAttributeTests : AttributeTestBase
{
    private class TestClass
    {
        [NotEmptyString]
        public string Name { get; set; }
    }

    [Test]
    public void ValidationShouldPass()
    {
        var test = new TestClass
        {
            Name = "foobar",
        };
        var res = Validate(test);
        Assert.That(res, Is.Empty);
    }

    [Test]
    public void ValidationShouldFailWhenStringIsEmpty()
    {
        var test = new TestClass
        {
            Name = string.Empty,
        };
        var res = Validate(test);
        Assert.That(res.First().ErrorMessage, Contains.Substring("Name is required and cannot be empty."));
    }

    [Test]
    public void ValidationShouldFailWhenStringIsNull()
    {
        var test = new TestClass
        {
            Name = null,
        };
        var res = Validate(test);
        Assert.That(res.First().ErrorMessage, Contains.Substring("Name is required and cannot be empty."));
    }

    [Test]
    public void ValidationShouldFailWhenStringIsNotProvided()
    {
        var test = new TestClass();
        var res = Validate(test);
        Assert.That(res.First().ErrorMessage, Contains.Substring("Name is required and cannot be empty."));
    }
}
