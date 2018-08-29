#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using dnlib.IO;

#endregion

namespace dnlib.DotNet.Pdb.Managed
{
    internal sealed class DbiScope : ISymbolScope
    {
        public DbiScope(string name, uint offset, uint length)
        {
            Name = name;
            BeginOffset = offset;
            EndOffset = offset + length;

            Namespaces = new List<DbiNamespace>();
            Variables = new List<DbiVariable>();
            Children = new List<DbiScope>();
        }

        public string Name
        {
            get;
        }

        public uint BeginOffset
        {
            get;
            internal set;
        }

        public uint EndOffset
        {
            get;
            internal set;
        }

        public IList<DbiNamespace> Namespaces
        {
            get;
        }

        public IList<DbiVariable> Variables
        {
            get;
        }

        public IList<DbiScope> Children
        {
            get;
        }

        public void Read(RecursionCounter counter, IImageStream stream, uint scopeEnd)
        {
            if(!counter.Increment())
                throw new PdbException("Scopes too deep");

            while(stream.Position < scopeEnd)
            {
                var size = stream.ReadUInt16();
                var begin = stream.Position;
                var end = begin + size;

                var type = (SymbolType) stream.ReadUInt16();
                DbiScope child = null;
                uint? childEnd = null;
                switch(type)
                {
                    case SymbolType.S_BLOCK32:
                    {
                        stream.Position += 4;
                        childEnd = stream.ReadUInt32();
                        var len = stream.ReadUInt32();
                        var addr = PdbAddress.ReadAddress(stream);
                        var name = PdbReader.ReadCString(stream);
                        child = new DbiScope(name, addr.Offset, len);
                        break;
                    }
                    case SymbolType.S_UNAMESPACE:
                        Namespaces.Add(new DbiNamespace(PdbReader.ReadCString(stream)));
                        break;
                    case SymbolType.S_MANSLOT:
                    {
                        var variable = new DbiVariable();
                        variable.Read(stream);
                        Variables.Add(variable);
                        break;
                    }
                }

                stream.Position = end;
                if(child != null)
                {
                    child.Read(counter, stream, childEnd.Value);
                    Children.Add(child);
                    child = null;
                }
            }
            counter.Decrement();
            if(stream.Position != scopeEnd)
                Debugger.Break();
        }

        #region ISymbolScope

        int ISymbolScope.StartOffset => (int) BeginOffset;

        int ISymbolScope.EndOffset => (int) EndOffset;

        ISymbolScope[] ISymbolScope.GetChildren()
        {
            var scopes = new ISymbolScope[Children.Count];
            for(var i = 0; i < scopes.Length; i++)
                scopes[i] = Children[i];
            return scopes;
        }

        ISymbolVariable[] ISymbolScope.GetLocals()
        {
            var vars = new ISymbolVariable[Variables.Count];
            for(var i = 0; i < vars.Length; i++)
                vars[i] = Variables[i];
            return vars;
        }

        ISymbolNamespace[] ISymbolScope.GetNamespaces()
        {
            var nss = new ISymbolNamespace[Namespaces.Count];
            for(var i = 0; i < nss.Length; i++)
                nss[i] = Namespaces[i];
            return nss;
        }

        ISymbolMethod ISymbolScope.Method
        {
            get { throw new NotImplementedException(); }
        }

        ISymbolScope ISymbolScope.Parent
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}