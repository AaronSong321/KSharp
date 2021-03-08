using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;
using KSharp;
using Microsoft.Scripting.Utils;
using Mono.Cecil.Cil;

namespace KSharpCompiler
{
    public class IndexStack<T>
    {
        private readonly List<T> storage = new List<T>();
        private readonly List<T> memory = new List<T>();
        private int index = -1;

        public void StoragePush(T elem)
        {
            storage.Add(elem);
            memory.Add(elem);
            ++index;
        }
        public void StoragePop()
        {
            memory.RemoveAt(^1);
            --index;
        }
        public void MemoryPush()
        {
            memory.Add(storage[++index]);
        }
        public void MemoryPop()
        {
            memory.RemoveAt(^1);
        }
        public void TempPush(T elem)
        {
            memory.Add(elem);
        }
        public void TempPop()
        {
            memory.RemoveAt(^1);
        }
        public IEnumerable<T> Traverse()
        {
            for (int i = memory.Count - 1; i >= 0; --i)
                yield return memory[i];
        }
        
        public T Top() => memory[^1];
        public bool Any() => index >= 0;
        public void ResetMemory() => index = -1;
    }

    [NoThreading]
    public class LocalScopeAgent: CompileUnitAgent
    {
        public LocalScopeAgent(CompileUnit compileUnit) : base(compileUnit) { }

        public readonly IndexStack<NamespaceDefinition> ns = new IndexStack<NamespaceDefinition>();
        public readonly IndexStack<TypeDefinition> types = new IndexStack<TypeDefinition>();
        public readonly IndexStack<MethodDescriber> methods = new IndexStack<MethodDescriber>();
        public readonly IndexStack<PropertyDescriber> properties = new IndexStack<PropertyDescriber>();


        public MethodDescriber MethodTop => methods.Top();
        public ILProcessor Il => methods.Top().method.Body.GetILProcessor();

        public void Clear()
        {
            ns.ResetMemory();
            types.ResetMemory();
            methods.ResetMemory();
            properties.ResetMemory();
        }
        public string? NsTop()
        {
            return ns.Any() ? ns.Top().FullName : null;
        }
        public void DefineNamespace(string name)
        {
            var n = new NamespaceDefinition(name, NsTop() is null?name:NameGenAgent.NamespaceQualify(NsTop()!, name));
            ns.StoragePush(n);
        }
        public void DefineType(string name, TypeAttributes attributes)
        {
            var type = Compiler.DefineType(NsTop()??"", name, attributes);
            types.StoragePush(type);
        }

        public void DefineMethod(string name, IEnumerable<ParameterDefinition> parameters, TypeReference returnType, MethodMutability mutability)
        {
            var method = new MethodDefinition(name, 0, returnType);
            var methodDescriber = new MethodDescriber(method, mutability);
            method.Parameters.AddRange(parameters);
            var declaringType = types.Top();
            method.DeclaringType = declaringType;
            lock (declaringType) {
                declaringType.Methods.Add(method);
            }
            methods.StoragePush(methodDescriber);
        }

        public void DefineField(string name, TypeReference type, FieldAttributes attributes)
        {
            var field = new FieldDefinition(name, attributes, type);
            var declaringType = types.Top();
            field.DeclaringType = declaringType;
            lock (declaringType) {
                declaringType.Fields.Add(field);
            }
        }

        public void DefineAutoProperty(string name, TypeReference type, PropertyAttributes attributes)
        {
            var prop = new PropertyDefinition(name, attributes, type);
            var declaringType = types.Top();
            prop.DeclaringType = declaringType;

            var fieldAttr = FieldAttributes.Private.SpecialName();
            var field = new FieldDefinition(NameGenAgent.GenerateFieldName(name), fieldAttr, type);
            properties.StoragePush(new PropertyDescriber(prop, field, true));

            var getter = new MethodDefinition(NameGenAgent.GenerateGetterName(name), MethodAttributes.Public, type);
            prop.GetMethod = getter;
            var getterDescriber = new MethodDescriber(getter, MethodMutability.Pure);
            methods.TempPush(getterDescriber);
            Il.LoadThis();
            Il.LoadField(field);
            Il.Return();
            methods.TempPop();

            var setter = new MethodDefinition(NameGenAgent.GenerateSetterName(name), MethodAttributes.Public, type);
            prop.SetMethod = setter;
            var setterDescriber = new MethodDescriber(setter, MethodMutability.Mutable);
            methods.TempPush(setterDescriber);
            Il.LoadThis();
            Il.LoadArgument(1);
            Il.StoreField(field);
            Il.Return();
            methods.TempPop();

            lock (declaringType) {
                declaringType.Properties.Add(prop);
                declaringType.Fields.Add(field);
            }
        }
        
        
    }
}
