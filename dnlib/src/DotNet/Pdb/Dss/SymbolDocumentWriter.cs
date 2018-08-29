#region

using System;
using System.Diagnostics.SymbolStore;

#endregion

namespace dnlib.DotNet.Pdb.Dss
{
    internal sealed class SymbolDocumentWriter : ISymbolDocumentWriter
    {
        public SymbolDocumentWriter(ISymUnmanagedDocumentWriter writer)
        {
            SymUnmanagedDocumentWriter = writer;
        }

        public ISymUnmanagedDocumentWriter SymUnmanagedDocumentWriter
        {
            get;
        }

        public void SetCheckSum(Guid algorithmId, byte[] checkSum)
        {
            SymUnmanagedDocumentWriter.SetCheckSum(algorithmId, (uint) (checkSum == null ? 0 : checkSum.Length), checkSum);
        }

        public void SetSource(byte[] source)
        {
            SymUnmanagedDocumentWriter.SetSource((uint) source.Length, source);
        }
    }
}