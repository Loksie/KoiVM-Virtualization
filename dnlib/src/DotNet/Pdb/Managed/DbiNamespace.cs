#region

using System;
using System.Diagnostics.SymbolStore;

#endregion

namespace dnlib.DotNet.Pdb.Managed
{
    internal sealed class DbiNamespace : ISymbolNamespace
    {
        public DbiNamespace(string ns)
        {
            Namespace = ns;
        }

        public string Namespace
        {
            get;
        }

        #region ISymbolNamespace

        public string Name => Namespace;

        public ISymbolNamespace[] GetNamespaces()
        {
            throw new NotImplementedException();
        }

        public ISymbolVariable[] GetVariables()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}