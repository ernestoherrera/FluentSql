using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.EntityMappers.Contracts
{
    public interface IPropertyMap
    {
        string Name { get;  }

        string ColumnName { get;  }

        bool IsPrimaryKey { get;  }

        bool IsAutoIncrement { get;  }

        bool HasDefault { get;  }

        PropertyInfo PropertyInfo { get;  }

        int Size { get;  }

        bool Ignored { get;  }

        bool IsReadOnly { get;  }

        bool IsTableField { get; set; }

        string ColumnAlias { get;  }

        int OrdinalPosition { get;  }
    }
}
