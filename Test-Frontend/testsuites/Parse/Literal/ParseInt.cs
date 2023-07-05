using KSharp.Compiler.Ast;

public class ParseInt
{
    private static Token B(string content) => new(content, new TestPos());

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("-1", -1)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("-2147483648", int.MinValue)]
    [InlineData("1_00___0", 1000)]
    public void ParseGoodDecimalInteger(string content, int value)
    {
        var (i, _) = LiteralTokenParser.ParseDecimalInt(B(content));
        checked
        {
            Assert.Equal(value, (int)i);
        }
    }

    [Theory]
    [InlineData("2147483648")]
    [InlineData("-2147483649")]
    public void ParseBadDecimalInteger(string content)
    {
        var (_, e) = LiteralTokenParser.ParseDecimalInt(B(content));
        Assert.NotNull(e);
    }
}