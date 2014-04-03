using SQLite.Net.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SQLite.Net.Platform.WindowsPhone71
{
    public class ReflectionServiceWP7 : IReflectionService
    {
        public IEnumerable<PropertyInfo> GetPublicInstanceProperties(Type mappedType)
        {
            if (mappedType == null)
            {
                throw new ArgumentNullException("mappedType");
            }
            return from p in mappedType.GetProperties()
                   where
                       ((p.GetGetMethod() != null && p.GetGetMethod().IsPublic) || (p.GetSetMethod() != null && p.GetSetMethod().IsPublic) ||
                        (p.GetGetMethod() != null && p.GetGetMethod().IsStatic) || (p.GetSetMethod() != null && p.GetSetMethod().IsStatic))
                   select p;
        }

        public object GetMemberValue(object obj, Expression expr, MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Property)
            {
                var m = (PropertyInfo)member;
                return m.GetValue(obj, null);
            }
            if (member.MemberType == MemberTypes.Field)
            {
                return Expression.Lambda(expr).Compile().DynamicInvoke();
            }
            throw new NotSupportedException("MemberExpr: " + member.MemberType);
        }
    }
}
