namespace KoiVM.Runtime.Execution
{
    internal unsafe struct TypedRefPtr
    {
        public void* ptr;

        public static implicit operator TypedRefPtr(void* ptr)
        {
            return new TypedRefPtr {ptr = ptr};
        }

        public static implicit operator void*(TypedRefPtr ptr)
        {
            return ptr.ptr;
        }
    }
}