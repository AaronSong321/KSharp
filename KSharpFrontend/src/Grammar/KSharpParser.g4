parser grammar KSharpParser;
options { tokenVocab=KSharpLexer; }

@header {
#pragma warning disable 3021
}

decl
: varDecl
;

expr
: IntegerLiteral # IntLit
| expr opMul expr # BinExpr
| expr opAdd expr # BinExpr
;

opMul: Mul;
opAdd: Add|Sub;

id: Identifier;

varDecl
: varMut id Eq expr
;

decls
: decl (NL+ decl)*
;

file
: NL* decls NL* EOF
;

exprSep
: NL+
;

varMut: VAL | VAR;