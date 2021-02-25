using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Jmas
{
    public static class ReflectionHelper
    {
        public const AttributeTargets FieProp = AttributeTargets.Field | AttributeTargets.Property;
        public const BindingFlags InstanceMemberFinder = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static void SetField(object obj, FieldInfo field, object value)
        {
            field.SetValue(obj, value, InstanceMemberFinder, null, CultureInfo.CurrentCulture);
        }
        public static void SetProperty(object obj, PropertyInfo property, object value)
        {
            property.SetValue(obj, value, InstanceMemberFinder, null, null, CultureInfo.CurrentCulture);
        }

        public static IEnumerable<MemberInfo> GetValueMember<T>(object obj, string memberName)
        {
            bool Filter(MemberInfo member)
            {
                return member.Name == memberName && (member is FieldInfo fi ? fi.FieldType.IsSubclassOf(typeof(T)) : member is PropertyInfo pi && pi.PropertyType.IsSubclassOf(typeof(T)));
            }
            return (obj.GetType().GetFields().Where(Filter) as IEnumerable<MemberInfo>).Concat(obj.GetType().GetProperties().Where(Filter));
        }
        public static void SetValueMember<T>(object obj, IEnumerable<MemberInfo> members, T value)
        {
            foreach (var memberInfo in members) {
                if (memberInfo is FieldInfo fi) {
                    SetField(obj, fi, value);
                }
                if (memberInfo is PropertyInfo pi) {
                    SetProperty(obj, pi, value);
                }
            }
        }

        public static bool IsAssignableTo(this Type type, Type tar)
        {
            return tar.IsAssignableFrom(type);
        }
    }
}
