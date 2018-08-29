#region

using System;
using System.Diagnostics;

#endregion

namespace KoiVM.Runtime.Execution.Internal
{
    // Unboxed VT is stored in these boxes, boxed VT is stored as raw object.

    internal interface IValueTypeBox
    {
        object GetValue();
        Type GetValueType();
        IValueTypeBox Clone();
    }

    internal struct ValueTypeBox<T> : IValueTypeBox
    {
        public ValueTypeBox(T value)
        {
            this.value = value;
        }

        private readonly T value;

        public object GetValue()
        {
            return value;
        }

        public Type GetValueType()
        {
            return typeof(T);
        }

        public IValueTypeBox Clone()
        {
            return new ValueTypeBox<T>(value);
        }
    }

    internal static class ValueTypeBox
    {
        public static IValueTypeBox Box(object vt, Type vtType)
        {
            Debug.Assert(vtType.IsValueType);
            var boxType = typeof(ValueTypeBox<>).MakeGenericType(vtType);
            return (IValueTypeBox) Activator.CreateInstance(boxType, vt);
        }

        public static object Unbox(object box)
        {
            if(box is IValueTypeBox)
                return ((IValueTypeBox) box).GetValue();
            return box;
        }
    }
}