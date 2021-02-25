parser grammar KSharpParser;

options { tokenVocab = KSharpLexer; }

// 1 file
file
: normalFile
| globalFile
;

normalFile: (usingDirectives directiveExtraDelimiter?)? sectionPause (filescopeNs directiveExtraDelimiter?)? sectionPause filescopeDeclares? sectionPause;

globalFile: (usingDirectives directiveExtraDelimiter?)? sectionPause globalAttributes? sectionPause expressions sectionPause;

// 2.1 identifier

identifier
: Identifier
| SoftKeyword
| AT (Identifier | SoftKeyword)
;

// 2.2 separators
commaSep: NL* COMMA NL*;
expSep
: SEMICOLON NL*
| NL+
;
tokenPause: NL*;
sectionPause: NL*;
directiveExtraDelimiter
: SEMICOLON 
;

// 3
// 3.1 types

type: typeExpression;
typeExpression: lowestPriorityTypeExpression;
lowestPriorityTypeExpression: postfixType;
//lowestPriorityTypeExpression: tupleType;
//
//tupleType
//: functionOrPointerType
//| tupleType OPASTEROID functionOrPointerType
//;

postfixType
: atomType
| postfixType typeExpressionPostfix;

atomType
: parenthesisedTypeExpression
| keywordType
| qualifiedTypeName
;

qualifiedTypeName: (GLOBAL DOT)? (genericableIdentifier DOT)* genericableIdentifier
;

parenthesisedTypeExpression: LPAREN NL* type NL* RPAREN;

keywordType
: BOOL
| BYTE
| CHAR
| INT16
| INT
| INT64
| UINT
| FLOAT
| DOUBLE
| OBJECT
| DYNAMIC
| STRING
| UNIT
;

typeExpressionPostfix
: pointerTypePostfix
| arrayTypePostfix
| nullableTypePostfix
;

pointerTypePostfix: OPASTEROID;

arrayTypePostfix: rankSpecifiers;

nullableTypePostfix: QUESTION | OPNOT;

rankSpecifiers: rankSpecifier+;

rankSpecifier: LSQUARE COMMA* RSQUARE;

typeArgumentList: OPLT NL* typeArguments NL* OPGT;

typeArguments: typeArgument (commaSep typeArgument)*;

typeArgument: type;

typeParameter: identifier;

// 3.2 arguments

argumentList
: LPAREN tokenPause RPAREN
| LPAREN tokenPause positionalArguments tokenPause RPAREN
| LPAREN tokenPause (positionalArguments commaSep)? namedArguments tokenPause RPAREN
;

positionalArguments
: positionalArgument 
| positionalArguments commaSep positionalArgument
;

positionalArgument: argument;

namedArguments
: namedArgument
| namedArguments commaSep namedArgument
;

namedArgument: argumentName NL* COLON NL* argument;

argumentName: identifier;

argument: argumentPassMode? expression;

argumentPassMode: IN | OUT;

// 3.3 expressions

primaryExpression
: literalExpression
| interpolatedString
| genericableIdentifier
| keywordExpression
| parenthesisedExpression
| flowExpression
;

// 3.3.1 basic expressions

expression: primaryExpression;

expressions
: expression
| expressions expSep expression
;

literalExpression: Literal;

interpolatedString: DOUBLEQUOTE interpStringPart* IS_DOUBLEQUOTE;
interpStringPart
: IS_EscapeChar
| IS_NormalString
| IS_DOUBLECURL
| interpValue
;

interpValue: IS_LCURL expression RCURL;

genericableIdentifier: identifier typeArgumentList?;

parenthesisedExpression: LPAREN NL* expression NL* RPAREN;

memberAccessSuffix: DOT genericableIdentifier;

//invocationSuffix
//: LPAREN NL* argumentList? NL* RPAREN
//| implicitParameterLambdaExpression
//| LPAREN NL* argumentList? NL* RPAREN NL* implicitParameterLambdaExpression
//;

elementAccessSuffix: DOT parameterPropertyName? LSQUARE NL* argumentList NL* RSQUARE;

parameterPropertyName
: identifier
;

keywordExpression
: thisAccess
| backupAccess
| valueAccess
| baseAccess
| AnonymousLambdaParameter
;

thisAccess: THIS;

backupAccess: FIELD;

valueAccess: VALUE;

baseAccess
: BASE DOT genericableIdentifier
| BASE DOT parameterPropertyName? LSQUARE NL* argumentList NL* RSQUARE
;

// 3.3.2 flow control expressions

flowExpression
: ifExpression
| forExpression
;

ifExpression
: ifClause tokenPause ifResult (tokenPause elifClause tokenPause ifResult)* (elseKeyword tokenPause elseResult)?
|;
ifClause: ifKeyword tokenPause binaryPredicate;
ifKeyword: IF | IFNOT;
binaryPredicate: expression;
ifResult
: THEN tokenPause expression
| LCURL tokenPause expressions tokenPause RCURL
;
elifClause: elifKeyword tokenPause binaryPredicate;
elifKeyword: ELIF | ELIFNOT;
elseKeyword: ELSE;
elseResult
: expression
| LCURL tokenPause expressions tokenPause RCURL
;

forExpression
: 

// 4.1 using

usingDirectives
: usingDirective
| usingDirectives expSep usingDirective
;

usingDirective
: usingNs
| usingStaticType
| usingTypeAlias
;

usingNs: USING qualifiedIdentifier;

qualifiedIdentifier
: identifier
| qualifiedIdentifier DOT identifier
;

usingStaticType: USING STATIC type;

usingTypeAlias: USING identifier ASSIGN type;

filescopeNs: NAMESPACE qualifiedIdentifier;

// 4.2 block declarations

filescopeDeclares
: filescopeDeclare
| filescopeDeclares sectionPause filescopeDeclare;

filescopeDeclare
: nsDeclare
| typeDeclare
| globalFunctionDeclare
;

nsDeclare: NAMESPACE qualifiedIdentifier tokenPause LCURL tokenPause nsScopeDeclares tokenPause RCURL;

nsScopeDeclares
: nsScopeDeclare
| nsScopeDeclares sectionPause nsScopeDeclare
;

nsScopeDeclare
: nsDeclare
| typeDeclare
;

typeDeclare
: classDeclare
;

classDeclare: CLASS identifier tokenPause LCURL tokenPause typeScopeDeclares tokenPause RCURL;

typeScopeDeclares
: typeScopeDeclare
| typeScopeDeclares sectionPause typeScopeDeclare
;

typeScopeDeclare
: typeDeclare
| methodDeclare
//| explicitInterfaceImplDeclare
;

// 4.3  function

globalFunctionDeclare
: normalFunctionDeclare
| operatorDeclare
| castDeclare
| infixDeclare
| extensionMethodDeclare
;

commonFunctionHeader: tokenPause parameterList tokenPause typeAnnotater? tokenPause;
commonFunctionBody
: LCURL tokenPause functionBody tokenPause RCURL
| DOUBLEARROW expression
;

methodDeclare: methodModifiers FUN memberName commonFunctionHeader commonFunctionBody; 

memberName
: identifier
| type DOT identifier
;

typeAnnotater: COLON tokenPause type;

normalFunctionDeclare: globalFunModifiers FUN identifier commonFunctionHeader commonFunctionBody;

operatorDeclare: globalFunModifiers OPERATOR overloadableOperator commonFunctionHeader commonFunctionBody;

overloadableOperator
: ovUnaryOperator
| ovBinaryOperator
;
ovUnaryOperator
: OPADD 
| OPSUB
| OPADDONE
| OPSUBONE
| TRUE
| FALSE
;

ovBinaryOperator
: OPADD
| OPSUB
| OPASTEROID
| OPSLASH
| OPGGT
| OPEQ
| OPNEQ
| OPGT
| OPLT
| OPGE
| OPLE
| KeywordOperator
;

castDeclare: globalFunModifiers type tokenPause (COLONGT | TILDEGT) tokenPause type tokenPause commonFunctionBody;

infixDeclare: globalFunModifiers INFIX FUN tokenPause identifier commonFunctionHeader commonFunctionBody;

extensionMethodDeclare: globalFunModifiers FUN type DOT identifier commonFunctionHeader commonFunctionBody;

parameterList: LPAREN (tokenPause parameters)? tokenPause RPAREN;

parameters
: fixedParameters
| fixedParameters commaSep parameterArray
| parameterArray
;

fixedParameters: fixedParameter (commaSep fixedParameter)*;

fixedParameter: attributes? NL* parameterModeModifier? parameterDeclarationBody;

parameterDeclarationBody
: parameterInvokeName parameterLocalName typeAnnotater? defaultArgument
| parameterInvokeName parameterLocalName typeAnnotater
| DISCARD typeAnnotater
;
parameterInvokeName: identifier | DISCARD;
parameterLocalName: identifier;

parameterModeModifier: REF|OUT|IN;

returnTypeModeModifier: REF| REF READONLY;

defaultArgument: ASSIGN expression;

parameterArray: attributes? NL* identifier COLON PARAMS type;

functionBody: expressions?;

// 4.4 modifiers

//funModifiers: globalFunModifiers | methodModifiers;

globalFunModifiers: externMod? asyncMod?;

methodModifiers: externMod? newMod? staticMod? overrideMod? asyncMod?;

methodModifier
: staticMod
| mutableMod
| overrideMod
;

externMod: EXTERN;
staticMod: STATIC;
mutableMod: MUTABLE | IMMUTABLE | PURE;
overrideMod: OVERRIDE | SEALED | OPEN;
newMod: NEW;
asyncMod: ASYNC;

// 5 attributes

globalAttributes: attribute;
attributes: attributeSection (NL* attributeSection)*;

attributeSection: LSQUARE attributeTargetSpecifier? attributeList COMMA? RSQUARE;

attributeTargetSpecifier: attributeTarget COLON;

attributeTarget: FIELD| PROP| FUN| INDEXER| EVENT;

attributeList: attribute (NL* COMMA NL* attribute)*;

attribute: attributeName attributeArguments?;

attributeName: qualifiedTypeName;

attributeArguments: argumentList;