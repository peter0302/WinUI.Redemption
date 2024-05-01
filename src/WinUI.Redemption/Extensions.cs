using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;
using WinRT;
using WinUI.Redemption.Internal;

namespace WinUI.Redemption
{
    /// <summary>
    /// Assorted extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Obtains a native pointer from an <see cref="IBuffer"/>, such as 
        /// what's exposed by a <see cref="WriteableBitmap"/>, which can then be
        /// used with other native code or Windows APIs like GDI+ or DirectX.
        /// </summary>
        /// <param name="rtBuffer">An <see cref="IBuffer"/>instance.</param>
        /// <returns>A native pointer.</returns>
        public static IntPtr ToIntPtr(this IBuffer rtBuffer)
        {
            var bba = rtBuffer.As<IBufferByteAccess>();
            bba.Buffer(out var ptr);
            return ptr;
        }
    }
}
