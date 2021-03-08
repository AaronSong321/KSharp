parser grammar KSharpParser;

options { tokenVocab = KSharpLexer; }

// 1 file
file
: normalFile
| globalFile
;

normalFile: (usingDirectives directiveExtraDelimiter?)? sectionPause (fileScopeNs directiveExtraDelimiter?)? sectionPause fileScopeDeclares? sectionPause;

globalFile: (usingDirectives directiveExtraDelimiter?)? sectionPause globalAttributes? sectionPause expressions sectionPause;

// 2.1 identifier

identifier
: Identifier
| SoftKeyword
| AT (Identifier | Keyword)
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
lowestPriorityTypeExpression: prefixType;
//lowestPriorityTypeExpression: tupleType;
//
//tupleType
//: functionOrPointerType
//| tupleType OPASTEROID functionOrPointerType
//;

prefixType
: postfixType 
| parenthesisedTypeExpression
;

postfixType
: innerPostfixType
| innerPostfixType typeExpressionOuterPostfix
;

innerPostfixType
: atomType
| innerPostfixType DOT atomType
;

atomType
: decltypeExpression
| keywordType
| gidType
;

decltypeExpression: DECLTYPE LPAREN tokenPause expression tokenPause RPAREN;
gidType: (gid DOT)* gid;

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

typeExpressionOuterPostfix
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
| gid
| keywordExpression
| parenthesisedExpression
| flowExpression
| localVarDeclare
;

// 3.3.1 basic expressions

expression: lowestPriorityExpression;

lowestPriorityExpression
: assignmentExpression;

assignmentExpression
: infixCallExpression
| infixCallExpression ASSIGN assignmentExpression
;

infixCallExpression
//: flowInvokeExpression
: prefixExpression
| infixCallExpression identifier prefixExpression
;

//invokeExpression
//: prefixExpression prefixExpression
//| prefixExpression prefixExpression prefixExpression
//| prefixExpression prefixExpression prefixExpression prefixExpression
//| prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression
//| prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression
//| prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression
//| prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression
//| prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression prefixExpression
//;
//invokeExpression: prefixExpression;

prefixExpression
: postfixExpression
| awaitExpression
| refExpression
| prefixUnaryOperator prefixExpression
;

awaitExpression: AWAIT prefixExpression;

refExpression: REF prefixExpression;

prefixUnaryOperator
: OPNOT
| OPADDONE
| OPSUBONE
// TILDE
;

postfixExpression
: primaryExpression
| postfixExpression postfixSuffix
;

postfixSuffix
: methodInvokeSuffix
| elementAccessSuffix
| memberAccessSuffix
| delegateinvokeSuffix
;


expressions
: expression
| expressions expSep expression
;

//literalExpression: Literal;

interpolatedString: DOUBLEQUOTE interpStringPart* IS_DOUBLEQUOTE;
interpStringPart
: IS_EscapeChar
| IS_NormalString
| IS_DOUBLELCURL
| interpValue
;

interpValue: IS_LCURL expression interpFormat? RCURL;

interpFormat: COLON interpFormatPart+;

interpFormatPart: ISF_NormalChar | ISF_DOUBLERCURL;

gid: identifier typeArgumentList?;

parenthesisedExpression: LPAREN NL* expression NL* RPAREN;

dotOperator
: DOT
| QUESTION DOT 
| OPNOT DOT 
| OPNOT OPNOT DOT 
;

memberAccessSuffix: dotOperator gid;

delegateinvokeSuffix
: argumentList
| argumentList? invokePostfixLambda
;

methodInvokeSuffix: memberAccessSuffix delegateinvokeSuffix;

elementAccessSuffix: dotOperator gid? LSQUARE NL* argumentList NL* RSQUARE;

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
: BASE DOT gid
| BASE DOT parameterPropertyName? LSQUARE NL* argumentList NL* RSQUARE
;

// 3.3.2 flow control expressions

flowExpression
: ifExpression
| forExpression
| blockExpression
| whileExpression
| returnExpression
| yieldExpression
;

ifExpression
: ifClause tokenPause controlBody (tokenPause elifClause tokenPause controlBody)* (elseKeyword tokenPause controlBody)?
;
ifClause: ifKeyword tokenPause binaryPredicate;
ifKeyword: IF | IFNOT;
binaryPredicate: expression;
controlBody
: blockExpression
| lambdaBody
;

elifClause: elifKeyword tokenPause binaryPredicate;
elifKeyword: ELIF | ELIFNOT;
elseKeyword: ELSE;

forExpression
: FOR tokenPause controlFlowVarDeclare tokenPause IN iteratedValue tokenPause controlBody
;
iteratedValue: expression;

// 3.3.3 var declare expression

varTypeDeclare: identifier localVarTypeAnnotator;
varValueDeclare
: identifier localVarTypeAnnotator tokenPause initValue
| nameOrDiscard tokenPause initValue;

nameOrDiscard: identifier | DISCARD;

initValue
: ASSIGN expression
;

controlFlowVarDeclare: varTypeDeclare;

localVarDeclare
: localVarModifiers varTypeDeclare
| localVarModifiers varValueDeclare
;
localVarModifiers: localVarMutabilityMod;
localVarMutabilityMod: MUTABLE | CONST | CONSTEXPR | CONSTEVAL | CONSTINIT;

blockExpression: LCURL tokenPause expressions? tokenPause RCURL;
lambdaBody: DOUBLEARROW tokenPause expression;

whileExpression
: WHILE binaryPredicate tokenPause controlBody
| WHILE tokenPause controlBody
;

returnExpression
: RETURN 
| RETURN expression
;

yieldExpression
: YIELD expression
| YIELD BREAK
;

// 3.3.4 lambda

invokePostfixLambda: lambdaExpression;

lambdaExpression
: FUN lambdaParameterList tokenPause lambdaBody
| FUN lambdaParameterList tokenPause blockExpression
| LCURL overloadableOperator RCURL
; 

lambdaParameterList
: LPAREN lambdaParameters RPAREN
;
lambdaParameters
: lambdaParameter?
| (lambdaParameter commaSep)* lambdaParameter
;
lambdaParameter: parameterMod nameOrDiscard;

// 3.3.5 literal expression

literalExpression
: integerLiteral
| realLiteral
| nullLiteral
| boolLiteral
;

integerLiteral
: IntegerLiteral 
| HexLiteral
| BinLiteral
;

realLiteral
: RealLiteral 
;

nullLiteral: NULL;

boolLiteral: TRUE | FALSE;

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

usingNs: USING qid;

qid
: identifier
| qid DOT identifier
;

usingStaticType: USING STATIC type;

usingTypeAlias: USING identifier ASSIGN type;

fileScopeNs: NAMESPACE qid;

// 4.2 block declarations

fileScopeDeclares
: fileScopeDeclare
| fileScopeDeclares sectionPause fileScopeDeclare;

fileScopeDeclare
: nsDeclare
| typeDeclare
| globalFunctionDeclare
;

nsDeclare: NAMESPACE qid tokenPause LCURL tokenPause nsScopeDeclares tokenPause RCURL;

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

classDeclare: classModifiers tokenPause CLASS identifier tokenPause LCURL tokenPause typeScopeDeclares tokenPause RCURL;

classModifiers: externMod? accessibilityMod? typeSealedMod?;

typeSealedMod: STATIC | ABSTRACT | SEALED | OPEN;

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

commonFunctionHeader: tokenPause parameterList tokenPause retTypeAnnotator? tokenPause;
commonFunctionBody
: blockExpression
;

methodDeclare: methodModifiers FUN memberName commonFunctionHeader commonFunctionBody; 

memberName
: identifier
| type DOT identifier
;

//typeAnnotator: COLON tokenPause type;
localVarTypeAnnotator: COLON tokenPause localVarRefMod type;
localVarRefMod: REF?;
parameterTypeAnnotator: COLON tokenPause parameterMod type;
parameterMod: (IN|OUT|REF)?;
retTypeAnnotator: COLON tokenPause retTypeMod type;
retTypeMod
: REF?
| REF READONLY
;

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

parameterList: LPAREN tokenPause parameters tokenPause RPAREN;

parameters
: fixedParameters?
| fixedParameters commaSep parameterArray
| parameterArray
;

fixedParameters: fixedParameter (commaSep fixedParameter)*;

fixedParameter: attributes? NL* parameterDeclarationBody;

parameterDeclarationBody
: parameterLocalName parameterTypeAnnotator? defaultArgument
| parameterLocalName parameterTypeAnnotator
| DISCARD parameterTypeAnnotator
;
parameterInvokeName: identifier | DISCARD;
parameterLocalName: identifier;

defaultArgument: ASSIGN expression;

parameterArray: attributes? NL* PARAMS parameterDeclarationBody;

// 4.4 modifiers

//funModifiers: globalFunModifiers | methodModifiers;

globalFunModifiers: externMod? asyncMod?;

methodModifiers: externMod? accessibilityMod? staticMod? newMod? mutableMod? overrideMod? asyncMod?;

accessibilityMod: PUBLIC | PROTECTED | PRIVATE | INTERNAL | INTERNAL PROTECTED | PRIVATE PROTECTED;
externMod: EXTERN;
staticMod: STATIC;
mutableMod: MUTABLE | IMMUTABLE | PURE | CONSTEXPR | CONSTEVAL;
overrideMod: OVERRIDE | SEALED | OPEN | ABSTRACT;
newMod: NEW;
asyncMod: ASYNC;

// 5 attributes

globalAttributes: attribute;
attributes: attributeSection (tokenPause attributeSection)*;

attributeSection: LSQUARE attributeTargetSpecifier? attributeList COMMA? RSQUARE;

attributeTargetSpecifier: attributeTarget COLON;

attributeTarget: FIELD| PROP | FUN | INDEXER | EVENT | RETURN;

attributeList: attribute (NL* COMMA NL* attribute)*;

attribute: attributeName attributeArguments?;

attributeName: type;

attributeArguments: argumentList;