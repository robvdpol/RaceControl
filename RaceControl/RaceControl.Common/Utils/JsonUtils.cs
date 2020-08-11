using Newtonsoft.Json;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RaceControl.Common.Utils
{
    public static class JsonUtils
    {
        public static string GetJsonPropertyName<T>(this Expression<Func<T, object>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null && expression.Body is UnaryExpression unaryExpression)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new InvalidOperationException("Expression must be a member expression");
            }

            var attribute = memberExpression.Member.GetAttribute<JsonPropertyAttribute>();

            if (attribute == null)
            {
                throw new ArgumentException("Property must have a JsonPropertyAttribute");
            }

            return attribute.PropertyName;
        }

        public static T GetAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        }
    }
}