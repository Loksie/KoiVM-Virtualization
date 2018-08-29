#region

using System;
using System.Reflection;
using KoiVM.Runtime.Execution.Internal;

#endregion

namespace KoiVM.Runtime.Execution
{
    internal class FieldRef : IReference
    {
        private readonly FieldInfo field;
        private readonly object instance;

        public FieldRef(object instance, FieldInfo field)
        {
            this.instance = instance;
            this.field = field;
        }

        public VMSlot GetValue(VMContext ctx, PointerType type)
        {
            var inst = instance;
            if(field.DeclaringType.IsValueType && instance is IReference)
                inst = ((IReference) instance).GetValue(ctx, PointerType.OBJECT).ToObject(field.DeclaringType);
            return VMSlot.FromObject(field.GetValue(inst), field.FieldType);
        }

        public unsafe void SetValue(VMContext ctx, VMSlot slot, PointerType type)
        {
            if(field.DeclaringType.IsValueType && instance is IReference)
            {
                TypedReference typedRef;
                ((IReference) instance).ToTypedReference(ctx, &typedRef, field.DeclaringType);
                field.SetValueDirect(typedRef, slot.ToObject(field.FieldType));
            }
            else
            {
                field.SetValue(instance, slot.ToObject(field.FieldType));
            }
        }

        public IReference Add(uint value)
        {
            return this;
        }

        public IReference Add(ulong value)
        {
            return this;
        }

        public void ToTypedReference(VMContext ctx, TypedRefPtr typedRef, Type type)
        {
            TypedReferenceHelpers.GetFieldAddr(ctx, instance, field, typedRef);
        }
    }
}