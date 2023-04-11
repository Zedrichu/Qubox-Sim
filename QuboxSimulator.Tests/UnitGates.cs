using System.Globalization;

namespace QuboxSimulator.Tests;

public class GateTests
{
    [SetUp]
    public void Setup()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}