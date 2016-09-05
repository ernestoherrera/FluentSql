using System;
using System.Collections.Generic;

namespace FluentSql.Mappers.Contracts
{
    interface IEntityMap
    {
        string Database { get;  }
        string SchemaName { get;  }
        string TableName { get;  }
        string TableAlias { get;  }
        List<PropertyMap> Properties { get;  }
        Type EntityType { get;  }
        string Name { get; }

    }
}
