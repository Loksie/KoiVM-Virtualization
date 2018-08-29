namespace dnlib.DotNet.Pdb.Managed
{
    internal struct DbiSourceLine
    {
        public DbiDocument Document;
        public uint Offset;
        public uint LineBegin;
        public uint LineEnd;
        public uint ColumnBegin;
        public uint ColumnEnd;
    }
}