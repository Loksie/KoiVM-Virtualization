#region

using System;
using System.Diagnostics.SymbolStore;

#endregion

namespace dnlib.DotNet.Pdb.Dss
{
    internal sealed class SymbolDocument : ISymbolDocument
    {
        public SymbolDocument(ISymUnmanagedDocument document)
        {
            SymUnmanagedDocument = document;
        }

        public ISymUnmanagedDocument SymUnmanagedDocument
        {
            get;
        }

        public Guid CheckSumAlgorithmId
        {
            get
            {
                Guid guid;
                SymUnmanagedDocument.GetCheckSumAlgorithmId(out guid);
                return guid;
            }
        }

        public Guid DocumentType
        {
            get
            {
                Guid guid;
                SymUnmanagedDocument.GetDocumentType(out guid);
                return guid;
            }
        }

        public bool HasEmbeddedSource
        {
            get
            {
                bool result;
                SymUnmanagedDocument.HasEmbeddedSource(out result);
                return result;
            }
        }

        public Guid Language
        {
            get
            {
                Guid guid;
                SymUnmanagedDocument.GetLanguage(out guid);
                return guid;
            }
        }

        public Guid LanguageVendor
        {
            get
            {
                Guid guid;
                SymUnmanagedDocument.GetLanguageVendor(out guid);
                return guid;
            }
        }

        public int SourceLength
        {
            get
            {
                uint result;
                SymUnmanagedDocument.GetSourceLength(out result);
                return (int) result;
            }
        }

        public string URL
        {
            get
            {
                uint count;
                SymUnmanagedDocument.GetURL(0, out count, null);
                var chars = new char[count];
                SymUnmanagedDocument.GetURL((uint) chars.Length, out count, chars);
                if(chars.Length == 0)
                    return string.Empty;
                return new string(chars, 0, chars.Length - 1);
            }
        }

        public int FindClosestLine(int line)
        {
            uint result;
            SymUnmanagedDocument.FindClosestLine((uint) line, out result);
            return (int) result;
        }

        public byte[] GetCheckSum()
        {
            uint bufSize;
            SymUnmanagedDocument.GetCheckSum(0, out bufSize, null);
            var buffer = new byte[bufSize];
            SymUnmanagedDocument.GetCheckSum((uint) buffer.Length, out bufSize, buffer);
            return buffer;
        }

        public byte[] GetSourceRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            uint bufSize;
            SymUnmanagedDocument.GetSourceRange((uint) startLine, (uint) startColumn, (uint) endLine, (uint) endColumn, 0, out bufSize, null);
            var buffer = new byte[bufSize];
            SymUnmanagedDocument.GetSourceRange((uint) startLine, (uint) startColumn, (uint) endLine, (uint) endColumn, (uint) buffer.Length, out bufSize, buffer);
            return buffer;
        }
    }
}