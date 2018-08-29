#region

using System;

#endregion

namespace dnlib.Threading
{
    /// <summary>
    ///     Cancellation token interface
    /// </summary>
    public interface ICancellationToken
    {
        /// <summary>
        ///     Throws a <see cref="OperationCanceledException" /> if the operation should be canceled
        /// </summary>
        void ThrowIfCancellationRequested();
    }
}