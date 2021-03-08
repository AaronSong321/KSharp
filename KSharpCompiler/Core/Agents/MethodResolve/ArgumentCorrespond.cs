using System.Collections.Generic;
using System.Linq;
using KSharp;
using Mono.Cecil;

namespace KSharpCompiler
{
    public struct ArgumentCorrespond
    {
        public readonly ArgumentType argument;
        public readonly ParameterResolveSignature parameter;
        public ConversionType? ConversionType { get; set; }

        public ArgumentCorrespond(ArgumentType argument, ParameterResolveSignature parameter)
        {
            this.argument = argument;
            this.parameter = parameter;
            ConversionType = null;
        }
    }

    public sealed class ArgumentCorrespondGroup
    {
        public readonly List<ArgumentCorrespond> corresponds;
        private CompilerMessage? errorMessage;
        public readonly MethodResolveSignature signature;

        public CompilerMessage? ErrorMessage {
            get => errorMessage;
            set => errorMessage ??= value;
        }

        public ArgumentCorrespondGroup(MethodResolveSignature signature)
        {
            corresponds = new List<ArgumentCorrespond>();
            this.signature = signature;
        }
        public ArgumentCorrespondGroup(MethodResolveSignatureShrink signature, List<ArgumentCorrespond> corresponds)
        {
            this.corresponds = corresponds;
            this.signature = signature;
        }
        public void Add(ArgumentCorrespond co)
        {
            corresponds.Add(co);
        }

        public ArgumentCorrespondGroup ShrinkOptional()
        {
            if (signature is MethodResolveSignatureExpand)
                return this;
            var noCorresponds = signature.parameters.Filter(t => corresponds.All(k => k.parameter != t));
            var sigNew = signature.Shrink(noCorresponds);
            return new ArgumentCorrespondGroup(sigNew, corresponds);
        }
        public void Reorder()
        {
            corresponds.Sort((a,b) => a.parameter.index - b.parameter.index);
        }

        public bool ConversionBetterThan(ArgumentCorrespondGroup o)
        {
            bool hasBetter = false;
            for (int i = 0; i < corresponds.Count; ++i) {
                if (((ConversionType)corresponds[i].ConversionType!).IsBetterThan((ConversionType)o.corresponds[i].ConversionType!))
                    hasBetter = true;
                if (((ConversionType)o.corresponds[i].ConversionType!).IsBetterThan((ConversionType)corresponds[i].ConversionType!))
                    return false;
            }
            return hasBetter;
        }

        public static bool GenericBetterThanNonGeneric(ArgumentCorrespondGroup a, ArgumentCorrespondGroup b)
        {
            return !a.signature.origin.ContainsGenericParameter && b.signature.origin.ContainsGenericParameter;
        }

        public static bool NormalFormBetterThanExpandedForm(ArgumentCorrespondGroup a, ArgumentCorrespondGroup b)
        {
            return !(a.signature is MethodResolveSignatureExpand) && b.signature is MethodResolveSignatureExpand;
        }

        public static bool LessExpandedParameters(ArgumentCorrespondGroup a, ArgumentCorrespondGroup b)
        {
            return a.signature.origin.Parameters.Count < b.signature.origin.Parameters.Count;
        }
    }
}
