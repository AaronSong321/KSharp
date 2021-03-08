lexer grammar KSharpLexer;

import UnicodeClasses;

@lexer::header{
using System.Collections.Generic;
}
@lexer::members{
int inInterpString;
Stack<int> curlyLevel = new Stack<int>();
}

DelimitedComment
: '/*' ( DelimitedComment | . )*? '*/'
  -> channel(HIDDEN)
;

LineComment
: '//' ~[\r\n]*
  -> channel(HIDDEN)
;

WS
: [\u0020\u0009\u000C]
  -> channel(HIDDEN)
;

NL: '\n' | '\r' '\n'? ;

fragment Hidden: DelimitedComment | LineComment | WS;

// 2 OPERATORS

ADDASSIGN: '+=';
SUBASSIGN: '-=';
MULTIASSIGN: '*=';
DIVIDEASSIGN: '/=';
MODASSIGN: '%=';
COLONGT: ':>';
TILDEGT: '~>';
DDOT: '..';
CCOLON: '::';
OPOR: '||';
OPAND: '&&';
DOUBLEQUESTION: '??';
OPGGT: OPGT OPGT;
OPEQ: '==';
OPNEQ: '!=';
OPLE: '<=';
OPGE: '>=';
OPADDONE: '++';
OPSUBONE: '--';
ARROW: '->';
DOUBLEARROW: '=>';

OPNOT: '!';
BACKSLASH: '\\';
LPAREN: '(';
RPAREN: ')';
LSQUARE: '[';
RSQUARE: ']';
LCURL: '{'{
if (inInterpString != 0) {
curlyLevel.Push(curlyLevel.Pop()+1);
}
};
RCURL: '}'{
if (inInterpString != 0) {
    int t = curlyLevel.Pop()-1;
    if (t is 0) {
    PopMode();
    }
    else {
    curlyLevel.Push(t);
    }
}
};
COLON: ':'{
if (inInterpString != 0) {
    if (curlyLevel.Peek() != 1) {
    throw new System.Exception();
    }
    PushMode(InterpStringFormat);
}
};
SEMICOLON: ';';
TILDE: '~';
COMMA: ',';
DOT: '.';
QUOTE: '\'';
DOUBLEQUOTE: '"' {
++inInterpString;
PushMode(InterpString);
};
ASSIGN: '=';
OPADD: '+';
OPSUB: '-';
OPASTEROID: '*';
OPSLASH: '/';
OPMOD: '%';
OPGT: '>';
OPLT: '<';
QUESTION: '?';
AT: '@';

// 3 KEYWORDS
// 3.1 TYPENAME
CLASS: 'class';
STRUCT: 'struct';
NAMESPACE: 'namespace';
INTERFACE: 'interface';
ENUM: 'enum';
SINGLETON: 'singleton';
RECORD: 'record';
UNION: 'union';
DELEGATE: 'delegate';

// 3.2 USING
USING: 'using';

// 3.3 type name alias
INT16: 'int16';
BYTE: 'byte';
INTPTR: 'int*';
INT: 'int';
INT64: 'int64';
UINT: 'uint';
STRING: 'string';
BOOL: 'bool';
VAR: 'var';
VAL: 'val';
DECIMAL: 'decimal';
CHAR: 'char';
DOUBLE: 'double';
FLOAT: 'float';
OBJECT: 'object';
DYNAMIC: 'dynamic';
UNIT: 'unit';

// 3.4 MODIFIER
// 3.4.1 Accessors

PUBLIC: 'public';
PROTECTED: 'protected';
INTERNAL: 'internal';
PRIVATE: 'private';

// 3.4.2 Staitc
STATIC: 'static';
CONST: 'const';

// 3.4.3 parameter modifier
OUT: 'out';
REF: 'ref';

// 3.4.4 override
OVERRIDE: 'override';
ABSTRACT: 'abstract';
VIRTUAL: 'virtual';
SEALED: 'sealed';
OPEN: 'open';
READONLY: 'readonly';
NEW: 'new';

// 3.5 Member definition context

FUN: 'fun'
{
};
FIELD: 'field'
{
};
PROP: 'property'
{
};
ADD: 'add';
REMOVE: 'remove';
EVENT: 'event';
INDEXER: 'indexer';
ACCESS: 'access'
{
};
WITH: 'with';
GET: 'get';
SET: 'set';
VALUE: 'value';
CTOR: 'ctor';
THIS: 'this';
BASE: 'base';
MUTABLE: 'mutable';
IMMUTABLE: 'immutable';
PURE: 'pure';
PARTIAL: 'partial';
CONSTEXPR: 'constexpr';
CONSTEVAL: 'consteval';
CONSTINIT: 'constinit';

// 3.6 Flow control
IF: 'if';
IFNOT: 'if!';
ELIF: 'elif';
ELIFNOT: 'elif!';
ELSE: 'else';
MATCH: 'match';
BREAK: 'break';
CONTINUE: 'continue';
YIELD: 'yield';
WHILE: 'while';
FOREACH: 'foreach';
DO: 'do';
GOTO: 'goto';
RETURN: 'return';
TRY: 'try';
CATCH: 'catch';
FINALLY: 'finally';

// 3.7 namespace
GLOBAL: 'global';

// 3.8 infix operator
IN: 'in';
NOTIN: '!in';
IS: 'is';
NOTIS: '!is';

// 3.9 lambda
AnonymousLambdaParameter: '$' AnonymousLambdaParameterPosition;
fragment AnonymousLambdaParameterPosition: '1'..'8';

// 3.10 Block guarder
CHECKED: 'checked';
UNCHECKED: 'unchecked';
UNSAFE: 'unsafe';

// 3.11 thread
AWAIT: 'await';
ASYNC: 'async';

// 3.12 assembly
ASSEMBLY: 'assembly';
MODULE: 'module';

// 3.13 method declaration
OPERATOR: 'operator';
INFIX: 'infix';
PARAMS: 'params';

// 3.14 expression 
DISCARD: '_';
DEFAULT: 'default';
EXTERN: 'extern';
FIXED: 'fixed';
LOCK: 'lock';
SIZEOF: 'sizeof';
STACKALLOC: 'stackalloc';
THROW: 'throw';
VOLATILE: 'volatile';
ALIAS: 'alias';
WHERE: 'where';
TYPEOF: 'typeof';

// 3.15 literal value
NULL: 'null';
TRUE: 'true';
FALSE: 'false';
ZERO: '0';

// 3.16 pointer type
MANAGED: 'managed';
UNMANAGED: 'unmanaged';


//3.16 relational pattern match
AND: 'and';
OR: 'or';
NOT: 'not';

// 3.17 keyword operators
BITAND: 'band';
BITOR: 'bor';
BITNOT: 'bnot';
BITXOR: 'bxor';
OPXOR: 'xor';
SHL: 'shl';
SHR: 'shr';

// 3.18 C++ keywords
DECLTYPE: 'decltype';

// 4 RESERVED_WORDS
// 4.1. reserved C# keywords
AS: 'as';
CASE: 'case';
EXPLICIT: 'explicit';
FOR: 'for';
IMPLICIT: 'implicit';
LONG: 'long';
SBYTE: 'sbyte';
SHORT: 'short';
SWITCH: 'switch';
USHORT: 'ushort';
VOID: 'void';

// 4.2 keyword assemble
Keyword: 
ABSTRACT|ACCESS|ALIAS|AND|BASE|BOOL|BREAK|BYTE|CATCH|CHAR|CHECKED|CLASS|CONST|CONSTEVAL|CONSTEXPR|CONTINUE|DECIMAL|DECLTYPE|DEFAULT|DELEGATE|DO|DOUBLE|DYNAMIC|ELIF|ELSE|ENUM|EVENT|EXTERN|FALSE|FINALLY|FIXED|FLOAT|FOREACH|FUN|GOTO|IF|IMMUTABLE|IN|INDEXER|INT|INT16|INT64|INTERFACE|INTERNAL|IS|LOCK|LONG|MANAGED|MUTABLE|NAMESPACE|NEW|NOT|NULL|OBJECT|OR|OPEN|OPERATOR|OUT|OVERRIDE|PARAMS|PARTIAL|PRIVATE|PROP|PROTECTED|PUBLIC|PURE|READONLY|REF|RETURN|SEALED|SHORT|SIZEOF|STACKALLOC|STATIC|STRING|STRUCT|SWITCH|THIS|THROW|TRUE|UINT|UNCHECKED|UNMANAGED|UNSAFE|TYPEOF|USING|VAL|VALUE|VAR|VIRTUAL|VOID|VOLATILE|WHILE|AS|CASE|EXPLICIT|FOR|IMPLICIT|LONG|SBYTE|SHORT|SWITCH|USHORT|VOID
;
SoftKeyword: KeywordOperator|
ABSTRACT|ACCESS|EXPLICIT|EXTERN|IMMUTABLE|IMPLICIT|INT16|INT64|MANAGED|MUTABLE|OPEN|OPERATOR|PARAMS|PURE|SEALED|UNMANAGED|UNSAFE|AS|CASE|LONG|SBYTE|SHORT|USHORT;
KeywordOperator:BITAND|BITOR|BITNOT|BITXOR|OPXOR|SHL|SHR;

// 5 LITERALS

//Literal
//: RealLiteral
//| IntegerLiteral
//| BooleanLiteral
//| NULL
//;
    
RealLiteral
: FloatLiteral
| DoubleLiteral
;

FloatLiteral
: DoubleLiteral [fF]
| DecDigits [fF]
;

fragment DecDigitOrSeparator: DecDigit | '_';

fragment DecDigits
: DecDigit DecDigitOrSeparator* DecDigit
| DecDigit
;
fragment DoubleExponent: [eE] [+-]? DecDigits;

DoubleLiteral
: DecDigits? '.' DecDigits DoubleExponent?
| DecDigits DoubleExponent
;

LongLiteral
: (IntegerLiteral | HexLiteral | BinLiteral) 'L'
;

IntegerLiteral
: DecDigitNoZero DecDigitOrSeparator* DecDigit
| DecDigit // including '0'
;

fragment UnicodeDigit
: UNICODE_CLASS_ND
;

fragment DecDigit
: '0'..'9'
;

fragment DecDigitNoZero
: '1'..'9'
;

fragment HexDigitOrSeparator
: HexDigit | '_'
;

HexLiteral
: '0' [xX] HexDigit HexDigitOrSeparator* HexDigit
| '0' [xX] HexDigit
;

fragment HexDigit
: [0-9a-fA-F]
;

fragment BinDigitOrSeparator
: BinDigit | '_'
;

BinLiteral
: '0' [bB] BinDigit BinDigitOrSeparator* BinDigit
| '0' [bB] BinDigit
;

fragment BinDigit
: [01]
;

BooleanLiteral: TRUE|FALSE;

Identifier
: (Letter | '_') (Letter | '_' | UnicodeDigit)*
| '`' ~('\r' | '\n' | '`' | '[' | ']' | '<' | '>')+ '`'
;

    
fragment Letter
: UNICODE_CLASS_LL
| UNICODE_CLASS_LM
| UNICODE_CLASS_LO
| UNICODE_CLASS_LT
| UNICODE_CLASS_LU
| UNICODE_CLASS_NL
;

fragment CommonCharacter
: SimpleEscapeSequence
| HexEscapeSequence
| UnicodeEscapeSequence
;

fragment SimpleEscapeSequence
: '\\\''
| '\\"'
| '\\\\'
| '\\0'
| '\\a'
| '\\b'
| '\\f'
| '\\n'
| '\\r'
| '\\t'
| '\\v'
;

fragment HexEscapeSequence
: '\\x' HexDigit
| '\\x' HexDigit HexDigit
| '\\x' HexDigit HexDigit HexDigit
| '\\x' HexDigit HexDigit HexDigit HexDigit
;

fragment UnicodeEscapeSequence
: '\\u' HexDigit HexDigit HexDigit HexDigit
| '\\U' HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit
;


//// 6 pp directives
//ENDIF: 'endif';
//OPSHARP: '#'
//{
//insidePpDirective = true;
//};
//LINE: 'line';
//KEYWORDHIDDEN: 'hidden';
//DEFINE: 'define';
//UNDEF: 'undef';
//ERROR: 'error';
//WARNING: 'warning';
//REGION: 'region';
//ENDREGION: 'endregion';
//PRAGMA: 'pragma';


// 7 interpolation string
mode InterpString;
IS_DOUBLELCURL: '{{';
IS_LCURL: '{' {
PushMode(DefaultMode);
curlyLevel.Push(1);
};
IS_DOUBLEQUOTE: '"'{
--inInterpString;
PopMode();
};
IS_EscapeChar : '\\\''
                | '\\"'
                | '\\\\'
                | '\\0'
                | '\\a'
                | '\\b'
                | '\\f'
                | '\\n'
                | '\\r'
                | '\\t'
                | '\\v'
                ;
IS_NormalString: /*{ !interpolatedVerbatium }?*/ ~('{'|'\\'|'"')+;
                
mode InterpStringFormat;
ISF_DOUBLERCURL: '}}';
ISF_RCURL: '}' {
PopMode();
PopMode();
curlyLevel.Pop();
};
ISF_NormalChar: ~('}');
