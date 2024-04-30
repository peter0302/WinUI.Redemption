using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

namespace WinUI.Redemption
{
    /// <summary>
    /// A binding source that is part of a 
    /// <see cref="MultiBinding"/>. A bug/quirk in
    /// WinUI appears to prevent defining <see cref="BindingBase"/>s
    /// as direct members of a collection in XAML.
    /// </summary>
    [ContentProperty(Name = nameof(Binding))]
    public class MultiBindingSource
    {
        /// <summary>
        /// The <see cref="Microsoft.UI.Xaml.Data.Binding"/> instance.
        /// Note all binding types and options are supported except for
        /// <see cref="BindingMode.TwoWay"/>.
        /// </summary>
        public Binding Binding { get; set; }
    }
}
