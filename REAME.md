# KSharp
Self-concoct programming language K#, general-purpose object-oriented programming language, based on features of C#, Kotlin, F#, and Swift
Syntax: Refer to KSharp\KSharpCompiler\Grammar

## Assembly info

K# is built with these tools:

- C# 8
- .NET 3.1 (sadly Mono.cecil does not yet support .NET 5.0 cross-platform)
- Anltr4, Antlr4.Runtime (lexer, parser generation)
- Mono.Cecil (emit IL code)

## Project structure

- KSharp: Standard K\# libraries used both by K# Compiler and any K# projects.
- KSharpCompiler: K# compiler source code
    - Commons: Some reusable code, not project-oriented
    - ContextExtension: Extensions on contexts built by Antlr.
    - Grammar: K# grammar written in Antlr.
    - GrammarChecker: unused at the moment
    - MemberDefinition: core classes of the compiler
    - OverloadResolve: unused at the moment
    - Program: a pseudo main function to call the compiler.

