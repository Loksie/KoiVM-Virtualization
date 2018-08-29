namespace KoiVM.Runtime.Execution
{
    internal enum ExecutionState
    {
        Next,
        Exit,
        Throw,
        Rethrow
    }
}