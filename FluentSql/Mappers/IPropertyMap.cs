using System.Reflection;

namespace FluentSql.Mappers.Contracts
{
    public interface IPropertyMap
    {
        string Name { get;  }

        string ColumnName { get;  }

        string ColumnDataType { get; }

        bool IsPrimaryKey { get;  }

        bool IsAutoIncrement { get;  }

        bool HasDefault { get;  }

        PropertyInfo PropertyInfo { get;  }

        int? Size { get;  }

        int? NumericPrecision { get; }

        int? NumericScale { get; }

        bool Ignored { get;  }

        bool IsComputed { get; }

        bool IsReadOnly { get;  }

        bool IsTableField { get; set; }

        string ColumnAlias { get;  }

        int OrdinalPosition { get;  }
    }
}
