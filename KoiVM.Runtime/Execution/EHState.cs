namespace KoiVM.Runtime.Execution
{
    internal class EHState
    {
        public enum EHProcess
        {
            Searching, // Search for handler, filter are executed
            Unwinding // Unwind the stack, fault/finally are executed
        }

        public int? CurrentFrame;

        public EHProcess CurrentProcess;
        public object ExceptionObj;
        public int? HandlerFrame;
        public VMSlot OldBP;
        public VMSlot OldSP;
    }
}