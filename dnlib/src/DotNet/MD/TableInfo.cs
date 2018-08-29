#region

using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace dnlib.DotNet.MD
{
    /// <summary>
    ///     Info about one MD table
    /// </summary>
    [DebuggerDisplay("{RowSize} {Name}")]
    public sealed class TableInfo
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="table">Table type</param>
        /// <param name="name">Table name</param>
        /// <param name="columns">All columns</param>
        public TableInfo(Table table, string name, IList<ColumnInfo> columns)
        {
            Table = table;
            Name = name;
            Columns = columns;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="table">Table type</param>
        /// <param name="name">Table name</param>
        /// <param name="columns">All columns</param>
        /// <param name="rowSize">Row size</param>
        public TableInfo(Table table, string name, IList<ColumnInfo> columns, int rowSize)
        {
            Table = table;
            Name = name;
            Columns = columns;
            RowSize = rowSize;
        }

        /// <summary>
        ///     Returns the table type
        /// </summary>
        public Table Table
        {
            get;
        }

        /// <summary>
        ///     Returns the total size of a row in bytes
        /// </summary>
        public int RowSize
        {
            get;
            internal set;
        }

        /// <summary>
        ///     Returns all the columns
        /// </summary>
        public IList<ColumnInfo> Columns
        {
            get;
        }

        /// <summary>
        ///     Returns the name of the table
        /// </summary>
        public string Name
        {
            get;
        }
    }
}