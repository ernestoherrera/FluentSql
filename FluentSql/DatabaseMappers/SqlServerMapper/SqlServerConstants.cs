using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.DatabaseMappers.SqlServerMapper
{
    internal class SqlServerConstants
    {
        public static string USER_TABLES_QUERY = @"SELECT	TABLE_CATALOG [Database], TABLE_NAME [Name], TABLE_SCHEMA [Schema], 
		                                                    CASE WHEN TABLE_TYPE = 'VIEW' THEN 1 ELSE 0 END [IsView]		
                                                    FROM  INFORMATION_SCHEMA.TABLES 
                                                    WHERE TABLE_TYPE='BASE TABLE' OR TABLE_TYPE='VIEW'
                                                    ORDER BY TABLE_CATALOG, TABLE_NAME;";

        public static string USER_COLUMNS_QUERY = @"SELECT	c.TABLE_CATALOG AS [Database],
		                                                    c.TABLE_SCHEMA AS [Schema], 
		                                                    c.TABLE_NAME AS [TableName], 
		                                                    c.COLUMN_NAME AS [ColumnName], 
		                                                    c.ORDINAL_POSITION AS OrdinalPosition, 
		                                                    CASE WHEN COLUMN_DEFAULT IS NULL THEN 0 ELSE 1 END AS HasDefault, 
		                                                    CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS IsNullable, 
		                                                    DATA_TYPE AS DataType, 
		                                                    CASE WHEN CHARACTER_MAXIMUM_LENGTH IS NULL THEN 0 ELSE CHARACTER_MAXIMUM_LENGTH END AS Size, 
		                                                    DATETIME_PRECISION AS DatePrecision,
		                                                    COLUMNPROPERTY(object_id('[' + c.TABLE_SCHEMA + '].[' + c.TABLE_NAME + ']'), c.COLUMN_NAME, 'IsIdentity') AS IsAutoIncrement,
		                                                    COLUMNPROPERTY(object_id('[' + c.TABLE_SCHEMA + '].[' + c.TABLE_NAME + ']'), c.COLUMN_NAME, 'IsComputed') as IsComputed,
		                                                    OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') IsPrimaryKey
                                                    FROM	INFORMATION_SCHEMA.COLUMNS c LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kc
			                                                    ON c.COLUMN_NAME = kc.COLUMN_NAME AND c.TABLE_NAME = KC.TABLE_NAME
                                                    ORDER BY	c.TABLE_CATALOG, c.TABLE_SCHEMA, c.TABLE_NAME, OrdinalPosition;";

        public static string USER_FOREIGN_KEYS_QUERY = @"SELECT OBJECT_NAME(pt.parent_object_id) TableName,
			                                                OBJECT_NAME(pt.constraint_object_id) ForeignKeyName,
			                                                 OBJECT_NAME(pt.referenced_object_id) ReferencedTable,
			                                                pc.name ReferencingColumn, 
			                                                rc.name ReferencedColumn
		                                                FROM sys.foreign_key_columns AS pt
		                                                INNER JOIN sys.columns AS pc
		                                                ON pt.parent_object_id = pc.[object_id]
		                                                AND pt.parent_column_id = pc.column_id
		                                                INNER JOIN sys.columns AS rc
		                                                ON pt.referenced_column_id = rc.column_id
		                                                AND pt.referenced_object_id = rc.[object_id]";
    }
}
