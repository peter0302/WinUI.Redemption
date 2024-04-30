using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUI.Redemption
{
    /// <summary>
    /// Interface which must be implemented by multi-binding
    /// converters assigned to <see cref="MultiBinding.Converter"/>.
    /// </summary>
    public interface IMultiValueConverter
    {
        object Convert(object[] values, object parameter);
    }
}
