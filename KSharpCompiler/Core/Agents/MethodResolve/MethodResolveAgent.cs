using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharp;
using KSharpCompiler.Grammar;
using Microsoft.Scripting.Metadata;
using Mono.Cecil;
using Mono.Collections.Generic;
using MetadataToken = Mono.Cecil.MetadataToken;


namespace KSharpCompiler
{
    public enum MethodMatchType
    {
        Perfect,
        Conversion
    }

    public class ArgumentType
    {
        public readonly TypeReference type;
        public readonly ArgumentPassMode passMode;
        public readonly int position;
        /// <summary>
        /// Store the value of the argument if it can be evaluated at compile-time
        /// Conversions like integer (0) to enumeration need this value
        /// </summary>
        public readonly object? constantValue;
        public readonly bool hasConstantValue;
        
        public ArgumentType(TypeReference type, ArgumentPassMode passMode, int position, object? constantValue, bool hasConstantValue)
        {
            this.type = type;
            this.passMode = passMode;
            this.position = position;
            this.constantValue = constantValue;
            this.hasConstantValue = hasConstantValue;
        }
        
    }

    public class NamedArgumentType : ArgumentType
    {
        public readonly string passName;
        public NamedArgumentType(TypeReference type, ArgumentPassMode passMode, int position, string passName, object? constantValue, bool hasConstantValue) : base(type, passMode, position, constantValue, hasConstantValue)
        {
            this.passName = passName;
        }
    }
    //
    // public sealed class AnonymousFunctionArgumentType : ArgumentType
    // {
    //     public AnonymousFunctionArgumentType(TypeReference type, ArgumentPassMode passMode) : base(type, passMode) { }
    // }

    public class MethodResolveAgent: CompilerAgent
    {

        public MethodResolveAgent(Compiler compiler) : base(compiler) { }

        /// <summary>
        /// Correspond arguments to parameters of a method 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="invokeKind"></param>
        /// <param name="positional"></param>
        /// <param name="named"></param>
        /// <param name="setterAdditionalArgument"></param>
        /// <returns></returns>
        private ArgumentCorrespondGroup Correspond(MethodResolveSignature method, MethodInvokeKind invokeKind, ArgumentType[] positional, NamedArgumentType[] named, ArgumentType? setterAdditionalArgument)
        {
            var parameters = method.parameters.ToHashSet();
            var g = new ArgumentCorrespondGroup(method);
            positional.ForEach(argument => {
                var k = parameters.FirstOrDefault(par => par.index == argument.position);
                if (k is null)
                    g.ErrorMessage = (MethodMatchFailure.NoParameterForArgument(argument.position));
                else {
                    g.Add(new ArgumentCorrespond(argument, k));
                    parameters.Remove(k);
                }
            });
            // TODO: params expand
            named.ForEach(argument => {
                var k = parameters.FirstOrDefault(par => par.name == argument.passName);
                if (k is null)
                    g.ErrorMessage = (MethodMatchFailure.NamedArgumentMissingParameter(argument.passName));
                else {
                    g.Add(new ArgumentCorrespond(argument, k));
                    parameters.Remove(k);
                }
            });
            if (setterAdditionalArgument != null && invokeKind.IsSetter()) {
                var k = parameters.First(par => par.name == "value");
                g.Add(new ArgumentCorrespond(setterAdditionalArgument, k));
                parameters.Remove(k);
            }
            var missingParameters = parameters.Where(par => !par.IsOptional).ToArray();
            if (missingParameters.Any())
                g.ErrorMessage = MethodMatchFailure.NoArgumentForParameters(missingParameters);
            return g;
        }

        private List<ArgumentCorrespondGroup> ResolveCandidates(MethodGroup methodGroup, MethodInvokeKind invokeKind, ArgumentType[] positional, NamedArgumentType[] named, ArgumentType? setterAdditionalArgument)
        {
            var matchGroup = new List<MethodResolveSignature>();
            matchGroup.AddRange(methodGroup.methods.Map(m => new MethodResolveSignature(m)));
            if (named.Length is 0)
                matchGroup.AddRange(matchGroup.Map(t => t.TryExpand(positional.Length)).NonNull());
            
            var correspondGroup = matchGroup.Map(method => Correspond(method, invokeKind, positional, named, setterAdditionalArgument));
            correspondGroup = correspondGroup.Filter(t => t.ErrorMessage is null);
            correspondGroup = correspondGroup.Filter(t => 
                 t.corresponds.All(argumentCorrespond => ArgumentPassModeExtensions.ByPointerModePerfectMatch(argumentCorrespond.argument, argumentCorrespond.parameter))
            );
            correspondGroup = correspondGroup.Filter(t => t.corresponds.All(correspond => {
                var b = Compiler.ConversionResolveAgent.GetConversionType(correspond.argument, correspond.parameter.type);
                correspond.ConversionType = b;
                if (correspond.parameter.attributes.IsByPointer() && b != ConversionType.Identity)
                    t.ErrorMessage = MethodMatchFailure.ByPointerParameterIsNotIdentical(correspond.argument.type, correspond.parameter.type, correspond.parameter.name??$"index {correspond.parameter.index}");
                else if (b == ConversionType.None)
                    t.ErrorMessage = MethodMatchFailure.NoConversion(correspond.argument.type, correspond.parameter.type, correspond.parameter.name??$"index {correspond.parameter.index}");
                return t.ErrorMessage is null;
            }));
            
            var removeList = new List<ArgumentCorrespondGroup>();
            removeList.AddRange(correspondGroup.Where(t => t.signature is MethodResolveSignatureExpand tt && correspondGroup.Any(k => k.signature == tt.unexpandedMethod)));
            removeList.ForEach(t => correspondGroup.Remove(t));

            correspondGroup = correspondGroup.Map(t => t.ShrinkOptional());
            correspondGroup.ForEach(t => t.Reorder());
            return correspondGroup;
        }
        
        private MethodResolveResult PickBetterFunctionMember(List<ArgumentCorrespondGroup> candidates)
        {
            MethodResolveResult? CheckNumber()
            {
                return candidates.Count switch {
                    0 => new MethodResolveResult(new MethodResolveFailure { Note = "No match" }),
                    1 => new MethodResolveResult(candidates),
                    _ => null
                };
            }

            var k2 = CheckNumber();
            if (k2 != null)
                return k2;
            foreach (var t1 in candidates) {
                if (candidates.All(t => ReferenceEquals(t, t1) || t1.ConversionBetterThan(t)))
                    return new MethodResolveResult(t1);
            }

            MethodResolveResult? Remove(Func<ArgumentCorrespondGroup, ArgumentCorrespondGroup, bool> func)
            {
                List<ArgumentCorrespondGroup> removeList = new List<ArgumentCorrespondGroup>();
                for (int i = 0; i < candidates.Count-1; ++i) {
                    for (int j = i + 1; j < candidates.Count; ++j) {
                        if (func(candidates[j], candidates[i])) {
                            removeList.Add(candidates[i]);
                            break;
                        }
                    }
                }
                removeList.ForEach(t => candidates.Remove(t));
                return CheckNumber();
            }
            
            var k = Remove(ArgumentCorrespondGroup.GenericBetterThanNonGeneric)??Remove(ArgumentCorrespondGroup.NormalFormBetterThanExpandedForm)??Remove(ArgumentCorrespondGroup.LessExpandedParameters);
            return k??new MethodResolveResult(candidates);
        }
        
        // private static bool MoreSpecific

        /// <summary>
        /// Resolve a method from a method group
        /// </summary>
        /// <param name="methodGroup"></param>Contains all methods with the correct name
        /// <param name="invokeKind"></param>
        /// <param name="positional"></param>
        /// <param name="named"></param>
        /// <param name="setterAdditionalArgument"></param>
        /// <returns></returns>
        public MethodResolveResult ResolveMethod(MethodGroup methodGroup, MethodInvokeKind invokeKind, ArgumentType[] positional, NamedArgumentType[] named, ArgumentType? setterAdditionalArgument = null)
        {
            var candidates = ResolveCandidates(methodGroup, invokeKind, positional, named, setterAdditionalArgument);
            return PickBetterFunctionMember(candidates);
        }
    }
}
