using System;
using System.Runtime.InteropServices;

namespace WinUI.Redemption.Internal
{
    [ComImport]
    [Guid("905A0FEF-BC53-11DF-8C49-001E4FC686DA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IBufferByteAccess
    {
        void Buffer(out IntPtr value);
    }
}