using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Collections.Generic;


namespace KSharpCompiler
{
    public class PropertyDescriber: MemberDescriber, IMemberDefinition
    {
        public readonly PropertyDefinition propertyDefinition;
        public readonly FieldDefinition? backup;
        public readonly bool isAuto;

        public PropertyDescriber(PropertyDefinition propertyDefinition, FieldDefinition backup, bool isAuto)
        {
            this.propertyDefinition = propertyDefinition;
            this.backup = backup;
            this.isAuto = isAuto;
        }
        public PropertyDescriber(PropertyDefinition propertyDefinition)
        {
            this.propertyDefinition = propertyDefinition;
            backup = null;
            isAuto = false;
        }
        
        public MetadataToken MetadataToken {
            get => propertyDefinition.MetadataToken;
            set => propertyDefinition.MetadataToken = value;
        }
        public Collection<CustomAttribute> CustomAttributes {
            get => propertyDefinition.CustomAttributes;
        }
        public bool HasCustomAttributes {
            get => propertyDefinition.HasCustomAttributes;
        }
        public string Name {
            get => ((IMemberDefinition)propertyDefinition).Name;
            set => ((IMemberDefinition)propertyDefinition).Name = value;
        }
        public string FullName {
            get => propertyDefinition.FullName;
        }
        public bool IsSpecialName {
            get => propertyDefinition.IsSpecialName;
            set => propertyDefinition.IsSpecialName = value;
        }
        public bool IsRuntimeSpecialName {
            get => propertyDefinition.IsRuntimeSpecialName;
            set => propertyDefinition.IsRuntimeSpecialName = value;
        }
        public TypeDefinition DeclaringType {
            get => propertyDefinition.DeclaringType;
            set => propertyDefinition.DeclaringType = value;
        }
    }
}
