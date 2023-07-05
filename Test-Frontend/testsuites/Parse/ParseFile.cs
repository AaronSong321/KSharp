global using Xunit;

public class ParseFile
{
    string[] args;
    public ParseFile()
    {
        args = Array.Empty<string>();
    }

    [Fact]
    void TestParse()
    {
        Assert.NotNull(args);
    }
}