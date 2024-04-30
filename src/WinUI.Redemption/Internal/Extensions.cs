using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;

namespace WinUI.Redemption.Internal
{
    internal static class Extensions
    {
        public static DependencyProperty TryGetDependencyProperty(this Type type, string propertyName)
        {
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
