using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WinUI.Redemption
{
    /// <summary>
    /// A collection of <see cref="MultiBinding"/>s which can be 
    /// assigned in XAML to the attached property
    /// <see cref="MultiBinding.MultiBindingsProperty"/>
    /// for relatively convenient XAML-only usage. Note that a
    /// <see cref="MultiBindingCollection"/> is best declared as
    /// as a resource and assigned to the 
    /// <see cref="MultiBinding.MultiBindingsProperty"/> attached
    /// property of the target as a <c>StaticResource</c> so it is 
    /// instantiated only once in the app lifecycle. This improves 
    /// performance when used in templates as it reduces the amount of 
    /// reflection needed to resolve the target properties.
    /// </summary>
    public class MultiBindingCollection : Collection<MultiBinding>
    {
    }
}