using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

namespace WinUI.Redemption
{
    /// <summary>
    /// Provides a workaround for WinUI 3's omission of a means to
    /// set bindings in Style Setters. In the main Style, assign an instance of 
    /// <see cref="BindingStyle"/>, populated with a collection of 
    /// <see cref="BindingSetter"/>s. to the <see 
    /// cref="BindingStyle.StyleProperty"/> attached property. 
    /// The <see cref="BindingSetter"/>s will be applied as new
    /// bindings on the target FrameworkElement and property, rather than
    /// the built-in WinUI behavior which is to attempt to apply the
    /// bindings using a Setter instance as the binding source.
    /// </summary>
    [ContentProperty(Name = nameof(Setters))]
    public class BindingStyle
    {
        /// <summary>
        /// The collection of <see cref="BindingSetter"/>'s.
        /// Normally this value is not assigned directly, but
        /// is populated through XAML.
        /// </summary>
        public Collection<BindingSetter> Setters
        {
            get => _setters ?? (_setters = new Collection<BindingSetter>());
            set => _setters = value;
        }

        #region SmartStyle Style attached property
        /// <summary>
        /// An attached DependencyProperty for getting or setting
        /// a <see cref="BindingStyle"/> on a FrameworkElement.
        /// </summary>
        public static readonly DependencyProperty StyleProperty = DependencyProperty.RegisterAttached(
            "Style",
            typeof(BindingStyle),
            typeof(BindingStyle),
            new PropertyMetadata(
                (BindingStyle)null,
                (obj, args) =>
                {
                    if (!(obj is FrameworkElement fe) || !(args.NewValue is BindingStyle style))
                        return;
                    foreach (var s in style.Setters)
                    {
                        if (string.IsNullOrEmpty(s.PropertyName))
                            throw new ArgumentNullException(nameof(s.PropertyName));
                        if (s.Binding == null)
                            throw new ArgumentNullException(nameof(s.Binding));
                        var dp = s.ResolveProperty(fe.GetType());
                        if (dp == null)
                            throw new InvalidOperationException(
                                $"Could not locate {s.PropertyName}Property on {fe.GetType()}; " +
                                $"did you forget to specify {nameof(s.PropertyOwner)}?");
                        BindingOperations.SetBinding(obj, dp, s.Binding);
                    }
                }));
        public static BindingStyle GetStyle(DependencyObject obj)
        {
            return (BindingStyle)obj.GetValue(StyleProperty);
        }
        public static void SetStyle(DependencyObject obj, BindingStyle style)
        {
            obj.SetValue(StyleProperty, style);
        }
        #endregion

        private Collection<BindingSetter> _setters;
    }
}
