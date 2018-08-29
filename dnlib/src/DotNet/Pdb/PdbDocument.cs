#region

using System;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;

#endregion

namespace dnlib.DotNet.Pdb
{
    /// <summary>
    ///     A PDB document
    /// </summary>
    [DebuggerDisplay("{Url}")]
    public sealed class PdbDocument
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public PdbDocument()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="symDoc">A <see cref="ISymbolDocument" /> instance</param>
        public PdbDocument(ISymbolDocument symDoc)
        {
            if(symDoc == null)
                throw new ArgumentNullException("symDoc");
            Url = symDoc.URL;
            Language = symDoc.Language;
            LanguageVendor = symDoc.LanguageVendor;
            DocumentType = symDoc.DocumentType;
            CheckSumAlgorithmId = symDoc.CheckSumAlgorithmId;
            CheckSum = symDoc.GetCheckSum();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="url">Document URL</param>
        /// <param name="language">Language. See <see cref="SymLanguageType" /></param>
        /// <param name="languageVendor">Language vendor. See <see cref="SymLanguageVendor" /></param>
        /// <param name="documentType">Document type. See <see cref="SymDocumentType" /></param>
        /// <param name="checkSumAlgorithmId">Checksum algorithm ID</param>
        /// <param name="checkSum">Checksum</param>
        public PdbDocument(string url, Guid language, Guid languageVendor, Guid documentType, Guid checkSumAlgorithmId, byte[] checkSum)
        {
            Url = url;
            Language = language;
            LanguageVendor = languageVendor;
            DocumentType = documentType;
            CheckSumAlgorithmId = checkSumAlgorithmId;
            CheckSum = checkSum;
        }

        /// <summary>
        ///     Gets/sets the document URL
        /// </summary>
        public string Url
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the language GUID. See <see cref="SymLanguageType" />
        /// </summary>
        public Guid Language
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the language vendor GUID. See <see cref="SymLanguageVendor" />
        /// </summary>
        public Guid LanguageVendor
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the document type GUID. See <see cref="SymDocumentType" />
        /// </summary>
        public Guid DocumentType
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the checksum algorithm ID
        /// </summary>
        public Guid CheckSumAlgorithmId
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets/sets the checksum
        /// </summary>
        public byte[] CheckSum
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Url ?? string.Empty).ToUpperInvariant().GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = obj as PdbDocument;
            if(other == null)
                return false;
            return (Url ?? string.Empty).Equals(other.Url ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }
}