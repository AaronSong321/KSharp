using System.Reflection;

namespace KSharp.Compiler.Diagnostic;

public sealed class CompilerIntegrationException : Exception
{
    public CompilerIntegrationException() { }
    public CompilerIntegrationException(string message) : base(message) { }
}

public abstract class Diagnostic
{
    public string? Content { get; set; }
}

public sealed class DiagnosticBag
{
    static DiagnosticBag() {
        #if DEBUG
        CheckDiagnosticInfoAttributeUniqueness();
        #endif
    }

    static void CheckDiagnosticInfoAttributeUniqueness() {
        bool compilerIntegrationError = false;
        var (diagInfoPairs, diagNullPairs) = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Diagnostic)) && !type.IsAbstract)
            .Select(type => (type, type.GetCustomAttribute<DiagnosticInfoAttribute>()))
            .Partition(r => r.Item2 != null);
        foreach (var (diag, _) in diagNullPairs) {
            compilerIntegrationError = true;
            Console.Error.WriteLine($"Type {diag} does not have a {nameof(DiagnosticInfoAttribute)} attribute.");
        }

        foreach (var diagInfoGroup in diagInfoPairs.GroupBy(r => r.Item2!.Code).Where(g => g.Count() > 1)) {
            compilerIntegrationError = true;
            Console.Error.WriteLine($"Types has the diagnostic code {diagInfoGroup.Key}: {string.Join(", ", diagInfoGroup.Select(t => t.type.FullName))}");
        }

        if (compilerIntegrationError)
            throw new CompilerIntegrationException("Compiler integration checked failed.");
    }

    private readonly LinkedList<Diagnostic> _diags = new();
    public void Add(Diagnostic diagnostic) { _diags.AddLast(diagnostic); }
}

public enum DiagnosticLevel
{
    Fatal, Error, Warning1, Warning2, Warning3, Info = 10
}

public sealed class DiagnosticInfoAttribute : Attribute
{
    public uint Code { get; }
    public DiagnosticLevel DefaultLevel { get; }

    public DiagnosticInfoAttribute(DiagnosticLevel level, uint code) {
        Code = code;
        DefaultLevel = level;
    }

    public uint Key => (uint)DefaultLevel * 10000 + Code;
}