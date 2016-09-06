# FluentSql for Dapper

FluentSql is a Micro ORM that creates Sql statements using a Sql like syntax in C#.

```
	var selectQuery = store.GetSelectQuery<Employee>()
                                    .JoinOn<Order>((e, o) => e.Id == o.EmployeeId)
                                    .Where<Employee, Order>((e, o) => o.Id >= 1 && o.OrderDate > startingOrderDate)
                                    .OrderBy(e => e.Username);

    var employeeSet = store.ExecuteQuery(selectQuery);
```

## Features

1. Models do not need not any attributes.
2. Use C# code to create your Sql statements.
3. It saves coding time by using one API for all queries.

