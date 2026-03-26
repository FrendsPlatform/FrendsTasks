using System.Linq;
using Frends.Common.Toolkit.Attributes;
using NUnit.Framework;

namespace Frends.Common.Toolkit.Tests.AttributesTests;

public class RequiredIfAttributeTests : AttributeTestBase
{
    private class TestClass
    {
        public bool Flag { get; set; }

        [RequiredIf(nameof(Flag), true)]
        public string Name { get; set; }
    }

    private class InvalidTestClass
    {
        [RequiredIf("NonExistentProperty", true)]
        public string Name { get; set; }
    }

    [Test]
    public void ShouldValidateWithSuccessWhenConditionMet()
    {
        var test = new TestClass
        {
            Flag = true,
            Name = "foobar",
        };
        var res = Validate(test);
        Assert.That(res, Is.Empty);
    }

    [TestCase("")]
    [TestCase(null)]
    public void ShouldValidateWithFailureWhenConditionMet(string name)
    {
        var test = new TestClass
        {
            Flag = true,
            Name = name,
        };
        var res = Validate(test);
        Assert.That(res.First().ErrorMessage, Contains.Substring("Name is required"));
    }

    [Test]
    public void ShouldSkipValidationWhenConditionIsNotMet()
    {
        var test = new TestClass();
        var res = Validate(test);
        Assert.That(res, Is.Empty);
    }

    [Test]
    public void ValidationShouldFailWhenConditionIsIncorrect()
    {
        var test = new InvalidTestClass();
        var res = Validate(test);
        Assert.That(res.First().ErrorMessage, Contains.Substring("Unknown property: NonExistentProperty"));
    }

}
