using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class SqlDbParameter : DbParameter
    {
        public SqlDbParameter(string name, object value, DbType? dbType = null, ParameterDirection? direction = ParameterDirection.Input, int? size = null)
        {
            ParameterName = name;
            Value = value;
            DbType = dbType ?? DbType.Object;
            Direction = direction.Value;
            Size = size ?? 0;
        }
        public override DbType DbType
        {
            get;
            set;
        }

        public override ParameterDirection Direction
        {
            get;
            set;
        }

        public override bool IsNullable
        {
            get;
            set;
        }

        public override string ParameterName
        {
            get;
            set;
        }

        public override int Size
        {
            get;
            set;
        }

        public override string SourceColumn
        {
            get;
            set;
        }

        public override bool SourceColumnNullMapping
        {
            get;
            set;
        }

        public override object Value
        {
            get;
            set;
        }

        public override void ResetDbType()
        {
        }
    }
}
