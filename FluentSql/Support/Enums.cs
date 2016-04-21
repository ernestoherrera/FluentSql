using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Support
{
    /// <summary>
    /// Direction of the working node
    /// </summary>
    internal enum ExpressionDirection { Right, Left };

    /// <summary>
    /// Describes the SQL join type
    /// </summary>
    public enum JoinType { Inner, Right, Left, Cross, FullOuter };

    /// <summary>
    /// Used to determine what type of parameter type
    /// is included in expressions like the where clause
    /// </summary>
    public enum ParameterType { Constant, EntityType, Value };
}
