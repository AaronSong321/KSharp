lexer grammar KSharpLexer;
import UnicodeClasses;

@header {
#pragma warning disable 3021
}

// SECTION: keywords
VAL: 'val';
VAR: 'var';

// SECTION: operators

Add: '+';
Sub: '-';
Mul: '*';

Eq: '=';
NL: '\r'? '\n';

// SECTION: literals

fragment DecDigit: '0'..'9';
fragment DecDigitNoZero: '1'..'9';
fragment DecDigitOrSeparator: DecDigit | '_';

fragment DecDigits
    : DecDigit DecDigitOrSeparator* DecDigit
    | DecDigit
    ;

fragment DoubleExponent: [eE] [+-]? DecDigits;

RealLiteral
    : FloatLiteral
    | DoubleLiteral
    ;

FloatLiteral
    : DoubleLiteral [fF]
    | DecDigits [fF]
    ;

DoubleLiteral
    : DecDigits? '.' DecDigits DoubleExponent?
    | DecDigits DoubleExponent
    ;

IntegerLiteral
    : DecDigitNoZero DecDigitOrSeparator* DecDigit
    | DecDigit
    ;

fragment HexDigit: [0-9a-fA-F];
fragment HexDigitOrSeparator: HexDigit | '_';

HexLiteral
    : '0' [xX] HexDigit HexDigitOrSeparator* HexDigit
    | '0' [xX] HexDigit
    ;

fragment BinDigit: [01];
fragment BinDigitOrSeparator: BinDigit | '_';

BinLiteral
    : '0' [bB] BinDigit BinDigitOrSeparator* BinDigit
    | '0' [bB] BinDigit
    ;

UnsignedLiteral
    : (IntegerLiteral | HexLiteral | BinLiteral) [uU] [lL]?
    ;

LongLiteral
    : (IntegerLiteral | HexLiteral | BinLiteral) [lL]
    ;

BooleanLiteral: 'true'| 'false';

NullLiteral: 'null';

CharacterLiteral
    : '\'' (EscapeSeq | ~[\n\r'\\]) '\''
    ;

// SECTION: lexicalIdentifiers

fragment UnicodeDigit: UNICODE_CLASS_ND;

Identifier
    : (Letter | '_') (Letter | '_' | UnicodeDigit)*
    | '`' ~([\r\n] | '`')+ '`'
    ;

fragment UniCharacterLiteral
    : '\\' 'u' HexDigit HexDigit HexDigit HexDigit
    ;

fragment EscapedIdentifier
    : '\\' ('t' | 'b' | 'r' | 'n' | '\'' | '"' | '\\' | '$')
    ;

fragment EscapeSeq
    : UniCharacterLiteral
    | EscapedIdentifier
    ;

// SECTION: characters

fragment Letter
    : UNICODE_CLASS_LU
    | UNICODE_CLASS_LL
    | UNICODE_CLASS_LT
    | UNICODE_CLASS_LM
    | UNICODE_CLASS_LO
    ;

// SECTION: strings

QUOTE_OPEN: '"' -> pushMode(LineString);

TRIPLE_QUOTE_OPEN: '"""' -> pushMode(MultiLineString);

mode LineString;

QUOTE_CLOSE
    : '"' -> popMode
    ;

LineStrText
    : ~('\\' | '"' | '$')+ | '$'
    ;

LineStrEscapedChar
    : EscapedIdentifier
    | UniCharacterLiteral
    ;

LineStrExprStart
    : '${' -> pushMode(DEFAULT_MODE)
    ;

mode MultiLineString;

TRIPLE_QUOTE_CLOSE
    : MultiLineStringQuote? '"""' -> popMode
    ;

MultiLineStringQuote
    : '"'+
    ;

MultiLineStrText
    :  ~('"' | '$')+ | '$'
    ;

MultiLineStrExprStart
    : '${' -> pushMode(DEFAULT_MODE)
    ;