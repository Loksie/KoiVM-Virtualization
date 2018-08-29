#region

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using KoiVM.Runtime.Execution.Internal;

#endregion

namespace KoiVM.Runtime.Execution
{
    internal class VMStack
    {
        private const int SectionSize = 6; // 1 << 6 = 64
        private const int IndexMask = (1 << SectionSize) - 1;
        private LocallocNode localPool;

        private readonly List<VMSlot[]> sections = new List<VMSlot[]>();
        private uint topPos;

        public VMSlot this[uint pos]
        {
            get
            {
                if(pos > topPos)
                    return VMSlot.Null;
                var sectionIndex = pos >> SectionSize;
                return sections[(int) sectionIndex][pos & IndexMask];
            }
            set
            {
                if(pos > topPos)
                    return;
                var sectionIndex = pos >> SectionSize;
                sections[(int) sectionIndex][pos & IndexMask] = value;
            }
        }

        public void SetTopPosition(uint topPos)
        {
            if(topPos > 0x7fffffff)
                throw new StackOverflowException();

            var sectionIndex = topPos >> SectionSize;
            if(sectionIndex >= sections.Count)
                do
                {
                    sections.Add(new VMSlot[1 << SectionSize]);
                } while(sectionIndex >= sections.Count);
            else if(sectionIndex < sections.Count - 2)
                do
                {
                    sections.RemoveAt(sections.Count - 1);
                } while(sectionIndex < sections.Count - 2);

            // Clear stack object references
            var stackIndex = (topPos & IndexMask) + 1;
            var section = sections[(int) sectionIndex];
            while(stackIndex < section.Length && section[stackIndex].O != null)
                section[stackIndex++] = VMSlot.Null;
            if(stackIndex == section.Length && sectionIndex + 1 < sections.Count)
            {
                stackIndex = 0;
                section = sections[(int) sectionIndex + 1];
                while(stackIndex < section.Length && section[stackIndex].O != null)
                    section[stackIndex++] = VMSlot.Null;
            }
            this.topPos = topPos;

            CheckFreeLocalloc();
        }

        private void CheckFreeLocalloc()
        {
            while(localPool != null && localPool.GuardPos > topPos)
                localPool = localPool.Free();
        }

        public IntPtr Localloc(uint guardPos, uint size)
        {
            var node = new LocallocNode
            {
                GuardPos = guardPos,
                Memory = Marshal.AllocHGlobal((int) size)
            };
            var insert = localPool;
            while(insert != null)
            {
                if(insert.Next == null || insert.Next.GuardPos < guardPos)
                    break;
                insert = insert.Next;
            }
            if(insert == null)
            {
                localPool = node;
            }
            else
            {
                node.Next = insert.Next;
                insert.Next = node;
            }
            return node.Memory;
        }

        public void FreeAllLocalloc()
        {
            var node = localPool;
            while(node != null)
                node = node.Free();
            localPool = null;
        }

        ~VMStack()
        {
            FreeAllLocalloc();
        }

        public void ToTypedReference(uint pos, TypedRefPtr typedRef, Type type)
        {
            if(pos > topPos)
                throw new ExecutionEngineException();
            var section = sections[(int) (pos >> SectionSize)];
            var index = pos & IndexMask;
            if(type.IsEnum)
                type = Enum.GetUnderlyingType(type);
            if(type.IsPrimitive || type.IsPointer)
            {
                section[index].ToTypedReferencePrimitive(typedRef);
                TypedReferenceHelpers.CastTypedRef(typedRef, type);
            }
            else
            {
                section[index].ToTypedReferenceObject(typedRef, type);
            }
        }

        private class LocallocNode
        {
            public uint GuardPos;
            public IntPtr Memory;
            public LocallocNode Next;

            ~LocallocNode()
            {
                if(Memory != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(Memory);
                    Memory = IntPtr.Zero;
                }
            }

            public LocallocNode Free()
            {
                if(Memory != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(Memory);
                    Memory = IntPtr.Zero;
                }
                return Next;
            }
        }
    }
}