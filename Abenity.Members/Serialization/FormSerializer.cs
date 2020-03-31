using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Abenity.Members.Serialization
{
    internal static class FormSerializer<TModel>
    {
        public static Dictionary<string, string> Serialize(TModel model) => typeof(TModel).GetProperties()
            .Where(HasAttribute<FormNameAttribute>)
            .ToDictionary(GetKey, p => GetValue(p, model));

        private static string GetKey(PropertyInfo property) => HasAttribute<FormNameAttribute>(property)
            ? GetAttribute<FormNameAttribute>(property).FormName
            : property.Name;

        private static string GetValue(PropertyInfo property, TModel model) => property.PropertyType == typeof(bool)
            ? ((bool)property.GetValue(model) ? "1" : "0")
            : property.GetValue(model)?.ToString();

        private static bool HasAttribute<TAttribute>(PropertyInfo property) where TAttribute : Attribute =>
            Attribute.IsDefined(property, typeof(TAttribute));

        private static TAttribute GetAttribute<TAttribute>(PropertyInfo property) where TAttribute : Attribute =>
            Attribute.GetCustomAttributes(property, typeof(TAttribute)).Cast<TAttribute>().FirstOrDefault();
    }
}
