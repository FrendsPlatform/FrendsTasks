using System.ComponentModel.DataAnnotations;
using Frends.Common.Toolkit.Handlers;
using NUnit.Framework;

namespace Frends.Common.Toolkit.Tests;

public class ValidationHandlerTests
{
    private class TestClass
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
    }


    [Test]
    public void BasicValidationShouldPass()
    {
        TestClass foobar = new()
        {
            Name = "foobar",
        };
        Assert.DoesNotThrow(() => ValidationHandler.Run(foobar));
    }

    [Test]
    public void ValidationShouldFailOnNullObject()
    {
        TestClass foobar = null;
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(foobar));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("Validated object can't be null!"));
    }

    [Test]
    public void ValidationShouldFailWhenNoObjectsProvided()
    {
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run());
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("You must provide objects to validate"));
    }

    [Test]
    public void ValidationShouldFailWhenObjectArrayIsNull()
    {
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(null));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("You must provide objects to validate"));
    }

    [Test]
    public void MultipleValidationMessagesAreReturned()
    {
        TestClass foobar = new();
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(foobar, null));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("Validated object can't be null!"));
        Assert.That(ex.Message, Contains.Substring("Name is required"));
    }
}
