#region

using System;
using System.Diagnostics.SymbolStore;
using dnlib.IO;

#endregion

namespace dnlib.DotNet.Pdb.Managed
{
    internal sealed class DbiDocument : ISymbolDocument
    {
        public DbiDocument(string url)
        {
            URL = url;
            DocumentType = SymDocumentType.Text;
        }

        public string URL
        {
            get;
        }

        public Guid Language
        {
            get;
            private set;
        }

        public Guid LanguageVendor
        {
            get;
            private set;
        }

        public Guid DocumentType
        {
            get;
            private set;
        }

        public Guid CheckSumAlgorithmId
        {
            get;
            private set;
        }

        public byte[] CheckSum
        {
            get;
            private set;
        }

        public void Read(IImageStream stream)
        {
            stream.Position = 0;
            Language = new Guid(stream.ReadBytes(0x10));
            LanguageVendor = new Guid(stream.ReadBytes(0x10));
            DocumentType = new Guid(stream.ReadBytes(0x10));
            CheckSumAlgorithmId = new Guid(stream.ReadBytes(0x10));

            var len = stream.ReadInt32();
            if(stream.ReadUInt32() != 0)
                throw new PdbException("Unexpected value");

            CheckSum = stream.ReadBytes(len);
        }

        #region ISymbolDocument

        Guid ISymbolDocument.CheckSumAlgorithmId => CheckSumAlgorithmId;

        Guid ISymbolDocument.DocumentType => DocumentType;

        byte[] ISymbolDocument.GetCheckSum()
        {
            return CheckSum;
        }

        Guid ISymbolDocument.Language => Language;

        Guid ISymbolDocument.LanguageVendor => LanguageVendor;

        string ISymbolDocument.URL => URL;

        int ISymbolDocument.FindClosestLine(int line)
        {
            throw new NotImplementedException();
        }

        byte[] ISymbolDocument.GetSourceRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            throw new NotImplementedException();
        }

        bool ISymbolDocument.HasEmbeddedSource
        {
            get { throw new NotImplementedException(); }
        }

        int ISymbolDocument.SourceLength
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}