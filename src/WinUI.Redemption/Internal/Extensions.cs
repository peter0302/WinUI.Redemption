using System;
using System.Reflection;

using Microsoft.UI.Xaml;

namespace WinUI.Redemption.Internal
{
    internal static class Extensions
    {
        public static DependencyProperty TryGetDependencyProperty(this Type type, string propertyName)
        {
            // In WinUI the DP instances of native objects are
            // exposed as properties rather than fields, I guess
            // because of COM and stuff.
            var dpProp = type.GetProperty(
                $"{propertyName}Property",
                BindingFlags.Static
                    | BindingFlags.FlattenHierarchy
                    | BindingFlags.Public);
            if (dpProp != null)
                return dpProp.GetValue(null) as DependencyProperty;

            var dpField = type.GetField(
                $"{propertyName}Property",
                BindingFlags.Static
                    | BindingFlags.FlattenHierarchy
                    | BindingFlags.Public);
            if (dpField != null)
                return dpField.GetValue(null) as DependencyProperty;

            return null;
        }
    }
}
