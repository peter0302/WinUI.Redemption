using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WinUI.Redemption
{
    /// <summary>
    /// A collection of <see cref="MultiBinding"/>s which can be 
    /// assigned in XAML to the attached property
    /// <see cref="MultiBinding.MultiBindingsProperty"/>
    /// for relatively convenient XAML-only usage. Note when
    /// used in a <see cref="MultiBindingCollection"/>, each
    /// <see cref="MultiBinding"/> must populate the
    /// <see cref="MultiBinding.PropertyName"/>
    /// </summary>
    public class MultiBindingCollection : Collection<MultiBinding>
    {
    }
}