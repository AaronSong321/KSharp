namespace KSharp.Compiler.Ast;

public abstract record SourcePos(int Line, int Column);

public sealed record SourceFilePos(string? File, int Line, int Column) : SourcePos(Line, Column);

public sealed record TestPos() : SourcePos(0, 0);

public sealed record SourceCommandPos(int Line, int Column) : SourcePos(Line, Column);
