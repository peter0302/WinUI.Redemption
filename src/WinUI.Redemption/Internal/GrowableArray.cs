using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUI.Redemption.Internal
{
    internal class GrowableArray<T>
    {
        private T[] _array;

        public GrowableArray(int initialCount = 10)
        {
            _array = new T[initialCount];
        }

        public int Count => this._array.Length;

        public T this[int index]
        {
            get
            {
                if (index > this._array.Length - 1)
                    return default;
                return this._array[index];
            }
            set
            {
                this.EnsureOrGrow(index);
                this._array[index] = value;
            }
        }

        private void EnsureOrGrow(int index)
        {
            if (index > this._array.Length - 1)
            {
                var newArray = new T[Math.Max(this._array.Length * 2, index + 1)];
                Array.Copy(this._array, 0, newArray, 0, this._array.Length);
                this._array = newArray;
            }
        }
    }
}
