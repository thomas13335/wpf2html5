using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Wpf2Html5.TypeSystem.Interface;
using Wpf2Html5.TypeSystem.Items;

namespace Wpf2Html5.TypeSystem
{
    static class Extensions
    {
        public static string ToSeparatorList(this IEnumerable<object> list, string sep = ", ")
        {
            var sb = new StringBuilder();
            foreach (var s in list)
            {
                if (0 < sb.Length) sb.Append(sep);
                sb.AppendFormat("{0}", s);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the qualified identifier a the given runtime type.
        /// </summary>
        /// <param name="type">The runtime type.</param>
        /// <returns>The corresponding string.</returns>
        public static string GetRName(this Type type)
        {
            string result;

            if (type.IsGenericType)
            {
                var sb = new StringBuilder();
                sb.Append(type.GetGenericTypeDefinition().FullName);
                if (type.GenericTypeArguments.Any())
                {
                    sb.Append("<");
                    sb.Append(type.GenericTypeArguments.Select(e => e.GetRName()).ToSeparatorList());
                    sb.Append(">");
                }

                result = sb.ToString();
            }
            else
            {
                result = type.FullName;
            }

            return result;
        }

        public static string GetCodeName(this Type type)
        {
            var name = type.Name;
            if(type.IsGenericType)
            {
                name = name.Split('`').First();
            }

            return name;
        }

        public static NativeTypeRef WithScriptReference(this NativeTypeRef ltype, Assembly assembly, string path)
        {
            ltype.AddScriptReference(assembly, path);
            return ltype;
        }

        public static NativeTypeRef WithUntranslatedMembers(this NativeTypeRef ltype, params string[] names)
        {
            ltype.SetDisableTranslation(names);
            return ltype;
        }

        public static ITypeItem GetLType(this ITypeContext c, Type type)
        {
            return c.TranslateRType(type, TranslateOptions.None);
        }

        public static ITypeItem TranslateRType(this ITypeContext c, Type type, bool add)
        {
            return c.TranslateRType(type, add ? TranslateOptions.Add : TranslateOptions.None);
        }
    }

    /// <summary>
    /// Provides public extension methods for type items.
    /// </summary>
    public static class PublicExtensions
    {
        /// <summary>
        /// Adds a runtime type to a type context.
        /// </summary>
        /// <param name="c">The type context.</param>
        /// <param name="type">The runtime type.</param>
        /// <returns>The corresponding type item.</returns>
        public static ITypeItem AddRType(this ITypeContext c, Type type)
        {
            return c.TranslateRType(type, TranslateOptions.Add);
        }

        /// <summary>
        /// True if the type item is a property.
        /// </summary>
        public static bool IsProperty(this ITypeItem ltype)
        {
            return ltype is Property;
        }

        /// <summary>
        /// True if this represents an enumeration type.
        /// </summary>
        public static bool IsEnumeration(this ITypeItem ltype)
        {
            return ltype is Enumeration;
        }
    }
}
