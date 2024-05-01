using System;

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;

using WinUI.Redemption.Internal;

namespace WinUI.Redemption
{
    /// <summary>
    /// A Setter-type object for assigning Binding's inside
    /// Styles. Used with <see cref="BindingStyle"/>.
    /// </summary>
    public class BindingSetter
    {
        /// <summary>
        /// The target <see cref="DependencyProperty"/> name, e.g.,
        /// <c>Background</c>. For an attached property, ensure the 
        /// <see cref="PropertyOwner"/> property is set, since this
        /// class doesn't have access to XAML namespaces. While this
        /// uses reflection the amount is negligible as the target property
        /// is cached after resolution.
        /// </summary>
        public string PropertyName
        {
            get;
            set;
        }

        /// <summary>
        /// References the DependencyProperty owner when attached
        /// properties are targeted. Otherwise the owner is assumed
        /// to be the type of the FrameworkElement being styled.
        /// </summary>
        public Type PropertyOwner
        {
            get;
            set;
        }

        /// <summary>
        /// A BindingBase. In XAML, use normal binding notation, e.g.,
        /// <c>"{Binding RelativeSource={RelativeSource Mode=Self}, Path=SomeOtherProperty}"</c>
        /// </summary>
        public BindingBase Binding
        {
            get;
            set;
        }

        internal DependencyProperty ResolveProperty(Type ownerType)
        {
            if (_resolvedProperty != null)
                return _resolvedProperty;
            return _resolvedProperty = (PropertyOwner ?? ownerType).TryGetDependencyProperty(this.PropertyName);
        }

        DependencyProperty _resolvedProperty;
    }
}
