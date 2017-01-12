# FluentSql for Dapper

FluentSql is a library that creates Sql statements using a Sql like syntax in C#. It uses Dapper ORM to execute its statements. The goal of this ORM is to provide an easy way to create and execute Sql statements allowing for fast data access development.

FluentSql is designed to work of the information that the system already provides. On start up, it reads the database(s) metadata and caches it for use at runtime. Being this way, there is no need to add/ update attributes in your models. This means less code and faster development times.

Your start up code can be as simple as this:
```
var mapper = new EntityMapper(dbConnection);
```
It supports more complex scenarios where one can supply: database(s) names, assemblies where the models are located, database mappers and others.

## Features

 1. Select, Update, Delete, and Insert queries.
 2. Use of Multiple Join method on Select, Update, and Delete queries.
 3. Use of Transactions
 4. Use of most common Functions for the where clause: Contains, StartsWith, EndsWith, DateTime.Now, Parenthesis Hierarchic.
 5. Use of OrdeBy methods that allow for grid queries.
 6. Models do not need any attributes.
 7. Use C# code to create your Sql statements.
 8. It saves coding time by using one API for all queries.
 9. Support for Multi-Database queries for the same server.
 10. Currently, it only supports Sql Server syntax.
 11. Runtime readonly dictionary with all database tables and columns information keyed by type.
 12. Clean and lean Sql statements.

## Get Started

Please take a look at the [Wiki] (https://github.com/ernestoherrera/FluentSql/wiki/Get-Started)

## Known Issues

- Does not support self join queries.
- Does not issue a 'IS NULL' sql when looking for NULL values
- Does not support any of the SqlServer Date functions
- Does not support Group By Clause.

## Help out

As an open source code project, contributing to enhance FluentSql is a key part to make great software. For a quick [Walk through the code follow this link.] (http://ernestoherrera.net/sql/statements/for/dapper/2016/11/28/fluentsql-for-net/) One can help with the following:

- Documentation
- Bug Reports
- Bug Fixes
- Feature Requests
- Feature Implementation
- Creating test cases
- Test implementations
- Code quality
- Sample applications

## Copyright

Copyright © Ernesto Herrera and contributors

## License

FluentSql is licensed under [Apache 2.0] (http://www.apache.org/licenses/LICENSE-2.0)
