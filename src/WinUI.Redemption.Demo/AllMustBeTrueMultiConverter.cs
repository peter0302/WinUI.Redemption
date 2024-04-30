using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUI.Redemption.Demo
{
    public class AllMustBeTrueMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, object parameter)
        {
            foreach (var value in values ?? Enumerable.Empty<object>())
            {
                if (!(value is bool b) || !b)
                    return false;
            }
            return true;
        }
    }
}
