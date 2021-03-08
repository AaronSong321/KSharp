using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSharpCompiler.Grammar;
using Mono.Cecil.Cil;

//
// namespace KSharpCompiler
// {
//     public partial class Emitter: CompileUnitAgent
//     {
//         private ILProcessor il = null!;
//
//         public Emitter(CompileUnit compileUnit) : base(compileUnit) { }
//
//         public void EmitVariableDefinition(KSharpParser.LocalVarDeclareContext c)
//         {
//             var f = c.varTypeDeclare();
//             if (f != null) {
//                 var type = f.localVarTypeAnnotator().type();
//                 // Compiler.TypeResolveAgent.Resolve
//             }
//         }
//     }
// }
