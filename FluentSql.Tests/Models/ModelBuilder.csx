#r "System.Core.dll"
#r "System.Xml.dll"
#r "System.Xml.Linq.dll"
#r "Microsoft.SqlServer.Smo"
#r "Microsoft.SqlServer.ConnectionInfo"
#r "Microsoft.SqlServer.Management.Sdk.Sfc"
#r "System.Data.Entity.Design"
#r "System.Globalization"
#r "C:\Source\FluentSql\FluentSql\FluentSql\bin\Debug\Dapper.dll"

using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Data;
using Dapper;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

// Set your variable values
//
// Sql Server name
var serverName = "localhost";
// The SQL username
var sqlLogin = "appUser";
// The SQL password
var sqlPassword = "3044171035Fluent";
// The SQL database to generate the POCOs for
var sqlDatabaseName = "FluentSqlTestDb";
// The namespace to apply to the generated classes
var classNamespace = "FluentSql.Tests.Models";
// The destination folder for the generated classes, relative to this file's location.
var destinationFolder = @"C:\Source\FluentSql\FluentSql\FluentSql.Tests\Models\";
var fileExtension = ".cs";
// Name of the one model to be refreshed. If empty string it will refresh all models
var mapSingleTable = "";

var sqlServerConnection = new SqlConnection($"Server={serverName};Database={sqlDatabaseName};User ID={sqlLogin};Password={sqlPassword};");

sqlServerConnection.Open();

var sqlTables = GetSqlServerTables(sqlServerConnection, sqlDatabaseName);
var sqlColumns = GetSqlServerColumns(sqlServerConnection, sqlDatabaseName);
var service = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(CultureInfo.CurrentCulture);


if (!string.IsNullOrEmpty(mapSingleTable))
{
    sqlTables = sqlTables.Where(t => t.Name == mapSingleTable).ToList();
}

foreach (Table table in sqlTables)
{
    var columns = sqlColumns.Where(c => c.TableName == table.Name).ToList();
    var tableName = service.Singularize(table.Name);
    var filePath = destinationFolder + tableName + fileExtension;

    SavePoco(tableName, filePath, columns);
}

void SavePoco(string className, string fileName, List<Column> columns)
{
    var builder = new StringBuilder();
    var invalidColumnNames = new List<string> { "class" };
    var invalidFirstLetters = new List<KeyValuePair<char, string>>
    {
        new KeyValuePair<char, string>('1', "one" ),
        new KeyValuePair<char, string>( '2', "two" ),
        new KeyValuePair<char, string>('3', "three" ),
        new KeyValuePair<char, string>('4', "four" ),
        new KeyValuePair<char, string>( '5', "five" ),
        new KeyValuePair<char, string>('6', "six" ),
        new KeyValuePair<char, string>('7', "seven" ),
        new KeyValuePair<char, string>('8', "eight" ),
        new KeyValuePair<char, string>('9', "nine" ),
        new KeyValuePair<char, string>('0', "zero" ),
    };

    var defaultKvP = default(KeyValuePair<char, string>);

    using (var streamWriter = new StreamWriter($"{fileName}"))
    {
        builder.AppendLine($"using System;");
        builder.AppendLine();
        builder.AppendLine($"namespace {classNamespace}");
        builder.AppendLine("{");
        builder.Append(' ', 4).AppendLine($"/// This file is auto-generated. Do not make manual changes to this file");
        builder.Append(' ', 4).AppendLine($"/// {className} entity");
        builder.Append(' ', 4).AppendLine($"public class {className}");
        builder.Append(' ', 4).AppendLine("{");

        var iCounter = 0;
        var columnsAdded = new List<string>();

        foreach (var col in columns)
        {
            iCounter++;

            var propType = GetDataType(col.DataType);
            var columnName = col.ColumnName;

            if (string.IsNullOrEmpty(propType)) continue;

            if (columnsAdded.Contains(columnName)) continue;

            if (col.IsNullable && propType != "string" && propType != "byte[]")
                propType += "?";

            if (invalidColumnNames.Contains(columnName.ToLower()))
                throw new Exception($"Invalid column name: {columnName}");

            var kvp = invalidFirstLetters.FirstOrDefault(vp => vp.Key == columnName[0]);

            if (!kvp.Equals(defaultKvP))
                throw new Exception($"Invalid column name: {columnName}");

            builder.Append(' ', 8).AppendLine($"public {propType} {columnName} {{ get; set; }} ");

            columnsAdded.Add(columnName);

            if (iCounter < columns.Count())
                builder.AppendLine();
        }


        builder.Append(' ', 4).AppendLine("}");

        builder.AppendLine("}");

        streamWriter.Write(builder.ToString());
        streamWriter.Close();
    }

}


List<Table> GetSqlServerTables(SqlConnection connection, string databaseName)
{

    var tablesSql = @"SELECT	TABLE_CATALOG [Database], TABLE_NAME [Name], TABLE_SCHEMA [Schema], 
		                                        CASE WHEN TABLE_TYPE = 'VIEW' THEN 1 ELSE 0 END [IsView]		
                                        FROM  INFORMATION_SCHEMA.TABLES 
                                        WHERE TABLE_TYPE='BASE TABLE'
                                        ORDER BY TABLE_CATALOG, TABLE_NAME;";

    connection.ChangeDatabase(databaseName);

    var tables = connection.Query<Table>(tablesSql);

    return tables.ToList();
}




List<Column> GetSqlServerColumns(SqlConnection connection, string databaseName)
{
    var columnsSql = @"SELECT	c.TABLE_CATALOG AS [Database],
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

    connection.ChangeDatabase(databaseName);

    var columns = connection.Query<Column>(columnsSql);

    return columns.ToList();
}

public string GetDataType(string sqlDataTypeName)
{
    switch (sqlDataTypeName.ToLower())
    {
        case "bigint":
            return "long";
        case "binary":
        case "image":
        case "varbinary":
            return "byte[]";
        case "bit":
            return "bool";
        case "char":
            return "string";
        case "date":
        case "datetime":
        case "smalldatetime":
            return "DateTime";
        case "decimal":
        case "money":
        case "numeric":
            return "decimal";
        case "float":
            return "Double";
        case "int":
            return "int";
        case "nchar":
        case "nvarchar":
        case "text":
        case "varchar":
        case "xml":
            return "string";
        case "real":
            return "Single";
        case "smallint":
            return "Int16";
        case "tinyint":
            return "byte";
        case "uniqueidentifier":
            return "Guid";

        default:
            return null;
    }
}


public class Table
{
    public List<Column> Columns;
    public string Database;
    public string Name;
    public string Schema;
    public bool IsView;
    public bool Ignore;

    public Column GetColumn(string columnName)
    {
        return Columns.Single(c => string.Compare(c.Name, columnName, StringComparison.CurrentCultureIgnoreCase) == 0);
    }

    public Column this[string columnName]
    {
        get
        {
            return GetColumn(columnName);
        }
    }
}

public class Column
{
    /// <summary>
    /// PropertyInfo Name
    /// </summary>
    public string Name;
    /// <summary>
    /// Database Column Name
    /// </summary>
    public string ColumnName;
    /// <summary>
    /// Database type
    /// </summary>
    public string DataType;
    /// <summary>
    /// C# database type
    /// </summary>
    public DbType DatabaseType;
    /// <summary>
    /// Returns true if Column is a Primary Key
    /// </summary>
    public bool IsPrimaryKey;
    /// <summary>
    /// Returns true if Column is Nullable
    /// </summary>
    public bool IsNullable;
    /// <summary>
    /// Return true if Column is AutoIncrement or Identity
    /// </summary>
    public bool IsAutoIncrement;
    /// <summary>
    /// Return true if Column has a default value on insert
    /// </summary>
    public bool HasDefault;
    /// <summary>
    /// Return Database Column size
    /// </summary>
    public int Size;
    /// <summary>
    /// User specific field used to ignore the column
    /// </summary>
    public bool Ignore;
    /// <summary>
    /// Return true if Column is the column is computed by the DB engine
    /// </summary>
    public bool IsComputed;
    /// <summary>
    /// Return true if Column is readonly
    /// </summary>
    public bool IsReadOnly;
    /// <summary>
    /// Column Ordinal Position within the database table
    /// </summary>
    public int OrdinalPosition;
    /// <summary>
    /// Returns the table name where the column is part of
    /// </summary>
    public string TableName;

}
