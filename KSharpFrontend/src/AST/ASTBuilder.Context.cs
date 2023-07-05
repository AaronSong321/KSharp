using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using KSharp.Compiler.Grammar;

namespace KSharp.Compiler.Ast;

public partial class AstBuilder
{
    public partial Chunk BuildChunk(FileContext context);

    private const string kVar = "var";
    private const string kVal = "val";

    class ExpressionVisitor
    {
        //public static Expressoin 
        public static Expression VisitExpr(ExprContext context)
        {
            if (context is BinExprContext b)
            {
                return VisitBinExpr(b);
            } else if (context is IntLitContext l)
            {
                return VisitIntLitContext(l);
            }
            throw new();
        }

        public static Expression VisitBinExpr(BinExprContext context)
        {
            NumericOperatorKind opkind;
            if (context.opMul() is not null) {
                opkind = NumericOperatorKind.Multiply;
            } else if (context.opAdd().Add() is not null) {
                opkind = NumericOperatorKind.Add;
            } else {
                opkind = NumericOperatorKind.Subtract;
            }
            return new BinaryExpression {
                Left = VisitExpr(context.expr(0)),
                Op = new NumOp {
                    OpertorKind = opkind
                },
                Right = VisitExpr(context.expr(1)),
                Token = new(context),
            };
        }

        static IntExpression VisitIntLitContext(IntLitContext context)
        {
            return new(new Token(context.start));
        }
    }

    class DeclVisitor
    {
        public static Decl VisitDeclContext(DeclContext context)
        {
            return VisitVarDeclContext(context.varDecl());
        }

        static VarDecl VisitVarDeclContext(VarDeclContext context)
        {
            VarDecl decl = new()
            {
                Identifier = context.id().GetText(),
                IsVar = context.varMut().GetText() == kVar,
                Token = new(context)
            };
            return decl;
        }
    }

    class FileListener
    {
        readonly Chunk chunk;
        readonly List<Decl> decls;

        FileListener(FileContext context)
        {
            chunk = new Chunk {
                Token = new(context)
            };
            decls = new();
        }

        public static Chunk Visit(FileContext context)
        {
            FileListener listener = new(context);
            listener.EnterFile(context);
            listener.ExitFile(context);
            return listener.chunk;
        }

        void EnterFile(FileContext context)
        {
            foreach (var decl in context.decls().decl())
            {
                decls.Add(DeclVisitor.VisitDeclContext(decl));
            }
        }
        void ExitFile(FileContext context)
        {

        }
    }
}