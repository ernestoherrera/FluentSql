using FluentSql.SqlGenerators;
using FluentSql.Support;
using FluentSql.Tests.Models;
using FluentSql.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace FluentSql.Tests.SelectStatement
{
    [TestClass]
    public class SelectStatementTest
    {
        private DbConnectionTest _dbConnection;

        public SelectStatementTest()
        {
            string connString = TestConstants.ServerPair + TestConstants.DatabasePair + 
                                TestConstants.UsernamePair + TestConstants.PasswordPair;

            _dbConnection = new DbConnectionTest(connString);
            new Bootstrap(_dbConnection);
        }

        #region Asynchronous Gets
        [Fact]
        public async void SelectAsyncWherePredicateIsConstant()
        {
            var entityStore = new EntityStore(_dbConnection);

            var employees = await entityStore.GetAsync<Employee>(emp => emp.Id == 1);

            Xunit.Assert.NotNull(employees);

            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public async void SelectAsyncConstant2()
        {
            var entityStore = new EntityStore(_dbConnection);

            var employees = await entityStore.GetAsync<Employee>(u => 1 == u.Id);

            Xunit.Assert.NotNull(employees);

            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());

            var employee = employees.FirstOrDefault();

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Steve");

        }

        [Fact]
        public async void SelectAsyncGetByKeyAnonymous()
        {
            var entityStore = new EntityStore(_dbConnection);

            var employee = await entityStore.GetByKeyAsync<Employee>(new { Id = 1 });

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Steve");
        }

        [Fact]
        public async void SelectAsyncGetByKeyEntity()
        {
            var entityStore = new EntityStore(_dbConnection);
            var employeeTemplate = new Employee { Id = 1 };
            var employee = await entityStore.GetByKeyAsync<Employee>(employeeTemplate);

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Steve");

        }

        [Fact]
        public async void SelectAsyncGetSingle()
        {
            var loginRequest = new LoginRequest { Username = "SRogers" };
            var entityStore = new EntityStore(_dbConnection);

            var employee = await entityStore.GetSingleAsync<Employee>(u => u.Username == loginRequest.Username);

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Steve");
        }

        [Fact]
        public async void SelectGetAllAsync()
        {
            var store = new EntityStore(_dbConnection);
            var employees = await store.GetAllAsync<Employee>();

            Xunit.Assert.NotNull(employees);
            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public async void SelectWithJoinOnAndTResultAsync()
        {
            var entityStore = new EntityStore(_dbConnection);
            var steve = new Employee { Id = 1, FirstName = "Steve" };
            var tResult = await entityStore.GetWithJoinAsync<Employee, Order, Order>((e, o) => e.Id == o.EmployeeId,
                                                            (e, o) => e.FirstName == steve.FirstName);

            Xunit.Assert.NotNull(tResult);

            var firstOrder = tResult.FirstOrDefault();

            Xunit.Assert.IsType<Order>(firstOrder);
        }

        #endregion

        #region synchronous tests
        [Fact]
        public void SelectByKey()
        {
            var store = new EntityStore(_dbConnection);
            var SteveEmployeeId = 1;

            var employee = store.GetByKey<Employee>(1);
            var employee1 = store.GetByKey<Employee>(new { Id = 1 });
            var employee2 = store.GetByKey<Employee>(new Employee { Id = 1, FirstName = "Steve", LastName = "Rogers" });
            var employee3 = store.GetByKey<Employee>(SteveEmployeeId);

            var order = store.GetByKey<Order>(1);
            var order1 = store.GetByKey<Order>(new { Id = 1 });

            Xunit.Assert.NotNull(employee);
            Xunit.Assert.NotNull(employee1);
            Xunit.Assert.NotNull(employee2);
            Xunit.Assert.NotNull(employee3);
            Xunit.Assert.True(employee.Id == 1);
            Xunit.Assert.True(employee1.Id == 1);
            Xunit.Assert.True(employee2.Id == 1);
            Xunit.Assert.True(employee3.Id == 1);

            Xunit.Assert.NotNull(order);
            Xunit.Assert.NotNull(order1);
            Xunit.Assert.True(order.Id == 1);
            Xunit.Assert.True(order1.Id == 1);
        }

        [Fact]
        public void SelectSingle()
        {
            var store = new EntityStore(_dbConnection);
            var customerId = 1;

            var customer = store.GetSingle<Customer>(c => c.Id == customerId);

            Xunit.Assert.NotNull(customer);
            Xunit.Assert.IsType<Customer>(customer);
            Xunit.Assert.True(customer.Id == customerId);
        }

        [Fact]
        public void SelectStatementWherePredicateIsFieldAccess()
        {
            var entityStore = new EntityStore(_dbConnection);
            var lastname = "Rogers";

            var employees = entityStore.Get<Employee>(p => p.LastName == lastname);

            Xunit.Assert.NotNull(employees);

            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public void SelectWithJoinOnAndTResult()
        {
            var entityStore = new EntityStore(_dbConnection);
            var steve = new Employee { Id = 1, FirstName = "Steve" };
            var tResult = entityStore.GetWithJoin<Employee, Order, Order>((e, o) => e.Id == o.EmployeeId,
                                                            (e, o) => e.FirstName == steve.FirstName);

            Xunit.Assert.NotNull(tResult);

            var firstOrder = tResult.FirstOrDefault();

            Xunit.Assert.IsType<Order>(firstOrder);
        }

        [Fact]
        public void SelectJoin()
        {
            var entityStore = new EntityStore(_dbConnection);

            var orderTuples = entityStore.GetAllWithJoin<Employee, Order>((e, o) => e.Id == o.EmployeeId && e.Id == 1);

            Xunit.Assert.NotNull(orderTuples);

            var tupleEmployee = orderTuples.FirstOrDefault().Item1;
            var tupleOrder = orderTuples.FirstOrDefault().Item2;

            Xunit.Assert.IsType<Employee>(tupleEmployee);

            Xunit.Assert.IsType<Order>(tupleOrder);
        }

        [Fact]
        public void SelectGetAll()
        {
            var store = new EntityStore(_dbConnection);
            var employees = store.GetAll<Employee>();

            Xunit.Assert.NotNull(employees);
            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public void SelectWithJoinAndWhereClause()
        {
            var entityStore = new EntityStore(_dbConnection);
            var tuples = entityStore.GetWithJoin<Employee, Order>((e, o) => e.Id == o.EmployeeId,
                                                                    (e, o) => e.Id == 1);

            Xunit.Assert.NotNull(tuples);

            var firstTuple = tuples.FirstOrDefault();
            var employee = firstTuple.Item1;
            var order = firstTuple.Item2;

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1);

            Xunit.Assert.IsType<Order>(order);

            Xunit.Assert.True(order.EmployeeId == 1);
        }
        #endregion

        [Fact]
        public void GetSelectQuery()
        {
            var store = new EntityStore(_dbConnection);
            var selectQuery = store.GetSelectQuery<Employee>();

            selectQuery.Where(e => ( e.Id >= 1 && e.LastName.StartsWith("s") ) ||
                                    (e.LastName.StartsWith("rog")) )
                        .OrderBy(e => e.LastName)
                        .OrderByDescending(e => e.FirstName);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.True(employeeSet.Count() >= 2);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());

            var employeeLastNameWithSp = employeeSet.FirstOrDefault(e => e.LastName.ToLower().StartsWith("s"));

            Xunit.Assert.NotNull(employeeLastNameWithSp);
        }

        [Fact]
        public void WhereClauseWithStringComparison()
        {
            var store = new EntityStore(_dbConnection);

            var emp = new Employee { Username = "srogers" };

            var selectQuery = store.GetSelectQuery<Employee>();

            //Equals Test
            var getWithConstant = store.Get<Employee>(e => (e.Id >= 1 && e.Username.Equals("srogers")));

            Xunit.Assert.NotNull(getWithConstant);

            var getWithMemberAccess = store.Get<Employee>(e => (e.Id >= 1 && e.Username.Equals(emp.Username)));

            Xunit.Assert.NotNull(getWithMemberAccess);

            var getWithFieldConstant = store.Get<Employee>(e => (e.Id >= 1 && e.Username.Equals(TestConstants.USERNAME)));

            Xunit.Assert.NotNull(getWithFieldConstant);

            //StartsWith Test
            getWithConstant = store.Get<Employee>(e => (e.Id >= 1 && e.Username.StartsWith("srog")));

            Xunit.Assert.NotNull(getWithConstant);

            getWithMemberAccess = store.Get<Employee>(e => (e.Id >= 1 && e.Username.StartsWith(emp.Username)));

            Xunit.Assert.NotNull(getWithMemberAccess);

            getWithFieldConstant = store.Get<Employee>(e => (e.Id >= 1 && e.Username.StartsWith(TestConstants.USERNAME)));

            Xunit.Assert.NotNull(getWithFieldConstant);

            //EndsWith Test
            getWithConstant = store.Get<Employee>(e => (e.Id >= 1 && e.Username.EndsWith("ers")));

            Xunit.Assert.NotNull(getWithConstant);

            getWithMemberAccess = store.Get<Employee>(e => (e.Id >= 1 && e.Username.EndsWith(emp.Username)));

            Xunit.Assert.NotNull(getWithMemberAccess);

            getWithFieldConstant = store.Get<Employee>(e => (e.Id >= 1 && e.Username.EndsWith(TestConstants.USERNAME)));

            Xunit.Assert.NotNull(getWithFieldConstant);
        }

        [Fact]
        public void GetSelectQueryWithJoin1()
        {
            var store = new EntityStore(_dbConnection);
            var startingOrderDate = new DateTime(2015, 12, 1);
            var selectQuery = store.GetSelectQuery<Employee>()
                                    .JoinOn<Order>((e, o) => e.Id == o.EmployeeId)
                                    .Where<Employee, Order>((e, o) => o.Id >= 1 && o.OrderDate > startingOrderDate)
                                    .OrderBy(e => e.Username);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.True(employeeSet.Count() >= 2);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());
        }

        [Fact]
        public void GetSelectQueryJoin()
        {
            var store = new EntityStore(_dbConnection);

            var selectQuery = store.GetSelectQuery<Order>()
                                    .JoinOn<Employee>((o, e) => e.Id == o.EmployeeId)
                                    .OrderBy(o => o.OrderDate)
                                    .OrderBy<Employee>(e => e.LastName);
                                    

            var orderEmployeeSet = store.ExecuteQuery<Order, Employee>(selectQuery);

            Xunit.Assert.NotNull(orderEmployeeSet);
            Xunit.Assert.True(orderEmployeeSet.Count() >= 2);

            var orderEmployeeTuple = orderEmployeeSet.FirstOrDefault();

            Xunit.Assert.NotNull(orderEmployeeTuple);

            var order = orderEmployeeTuple.Item1;

            Xunit.Assert.IsType<Order>(order);
            Xunit.Assert.IsType<Employee>(orderEmployeeTuple.Item2);
        }

        [Fact]
        public void GetSelectQueryWithJoin2()
        {
            var store = new EntityStore(_dbConnection);
            var startingOrderDate = new DateTime(2015, 12, 1);
            var selectQuery = store.GetSelectQuery<Employee>()
                        .JoinOn<Order>((e, o) => e.Id == o.EmployeeId && o.Id >= 1)
                        .JoinOn<Order, Customer>((o, c) => o.CustomerId == c.Id)
                        .Where<Order, Customer, Employee>((o, c, e) => o.OrderDate > startingOrderDate 
                                                && c.City == "Gainesville" && e.Id >= 1)
                        .OrderBy(e => e.Username);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.True(employeeSet.Count() >= 2);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());
        }

        [Fact]
        public void GetSelectQueryWithJoin3()
        {
            var store = new EntityStore(_dbConnection);
            var startingOrderDate = new DateTime(2015, 12, 1);
            var selectQuery = store.GetSelectQuery<Employee, Order>((e, o) => e.Id == o.EmployeeId && e.Id >= 1);

            selectQuery.Where<Order>((e, o) => o.Id >= 1 && o.OrderDate > startingOrderDate);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.True(employeeSet.Count() >= 2);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());
        }

        [Fact]
        public async void GetSelectQueryWithJoin2Async()
        {
            var store = new EntityStore(_dbConnection);
            var startingOrderDate = new DateTime(2015, 12, 1);
            var selectQuery = store.GetSelectQuery<Employee>()
                        .JoinOn<Order>((e, o) => e.Id == o.EmployeeId && o.Id >= 1)
                        .JoinOn<Order, Customer>((o, c) => o.CustomerId == c.Id)
                        .Where<Order, Customer, Employee>((o, c, e) => o.OrderDate > startingOrderDate
                                                && c.City == "Gainesville" && e.Id >= 1)
                        .OrderBy(e => e.Username);

            var employeeSet = await store.ExecuteQueryAsync(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.True(employeeSet.Count() >= 2);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());
        }

        [Fact]
        public void GetSelectQueryWithLeftJoin()
        {
            var store = new EntityStore(_dbConnection);
            var startingOrderDate = new DateTime(2015, 12, 1);
            var selectQuery = store.GetSelectQuery<Order>()
                                    .JoinOn<Employee>((o, e) => e.Id == o.EmployeeId && e.Id == 1, 
                                                        JoinType.Left)
                                    .Where((o) => o.Id >= 1 && o.OrderDate > startingOrderDate);

            var orderSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(orderSet);
            Xunit.Assert.True(orderSet.Count() >= 2);
            Xunit.Assert.IsType<Order>(orderSet.FirstOrDefault());
        }

        [Fact]
        public void GetSelectQueryWithRightJoin()
        {
            var store = new EntityStore(_dbConnection);
            var startingOrderDate = new DateTime(2015, 12, 1);
            var selectQuery = store.GetSelectQuery<Order>()
                                    .JoinOn<Employee>((o, e) => e.Id == o.EmployeeId && e.Id == 1, 
                                                        JoinType.Right)
                                    .Where((o) => o.Id >= 1 && o.OrderDate > startingOrderDate);

            var orderSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(orderSet);
            Xunit.Assert.True(orderSet.Count() == 2);
            Xunit.Assert.IsType<Order>(orderSet.FirstOrDefault());
        }

        [Fact]
        public void GetSelectQueryWithCrossJoin()
        {
            var store = new EntityStore(_dbConnection);

            var selectQuery = store.GetSelectQuery<Order>()
                                    .JoinOn<Employee>(null, JoinType.Cross)
                                    .Where<Order, Employee>((o, e) => e.Id == o.EmployeeId);

            var orderSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(orderSet);
            Xunit.Assert.True(orderSet.Count() >= 2);
            Xunit.Assert.IsType<Order>(orderSet.FirstOrDefault());
        }

        [Fact]
        public void GetSelectQueryWithFullJoin()
        {
            var store = new EntityStore(_dbConnection);

            var selectQuery = store.GetSelectQuery<Order>()
                                    .JoinOn<Employee>((o, e) => e.Id == o.EmployeeId,
                                                        JoinType.FullOuter);

            var orderSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(orderSet);
            Xunit.Assert.True(orderSet.Count() >= 2);
            Xunit.Assert.IsType<Order>(orderSet.FirstOrDefault());
        }

        [Fact]
        public void SeletTop()
        {
            var store = new EntityStore(_dbConnection);
            int skip = 10, take = 10;
            Expression<Func<Employee, object>> orderByExpression = (e => e.LastName);

            var selectQuery = store.GetSelectQuery<Employee>()
                                    .GetTopRows(skip + take)
                                    .Where(e => e.Id > 1)
                                    .OrderBy(orderByExpression);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);

            var returnSet = employeeSet.Skip(skip).Take(take);

            Xunit.Assert.NotNull(returnSet);
            Xunit.Assert.True(returnSet.Count() == take);
        }

        [Fact]
        public void SeletTop1()
        {
            var store = new EntityStore(_dbConnection);
            int skip = 10, take = 10;

            var selectQuery = store.GetSelectQuery<Employee>()
                                    .GetTopRows(skip + take)
                                    .Where(e => e.Id > 1)
                                    .OrderBy(e => e.LastName)
                                    .OrderBy(e => e.FirstName)
                                    .OrderByDescending(e => e.City);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);

            var returnSet = employeeSet.Skip(skip).Take(take);

            Xunit.Assert.NotNull(returnSet);
            Xunit.Assert.True(returnSet.Count() == take);
        }

        [Fact]
        public void SeletTop2()
        {
            var store = new EntityStore(_dbConnection);
            int skip = 10, take = 10;
            var sortOrder = new List<SortOrderField>
            {
                new SortOrderField (typeof(Employee), "LastName"),
                new SortOrderField (typeof(Employee), "FirstName"),
                new SortOrderField (typeof(Employee), "City", SortOrder.Descending)
            };

            var selectQuery = store.GetSelectQuery<Employee>()
                                    .GetTopRows(skip + take)
                                    .Where(e => e.Id > 1)
                                    .OrderBy(sortOrder);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);

            var returnSet = employeeSet.Skip(skip).Take(take);

            Xunit.Assert.NotNull(returnSet);
            Xunit.Assert.True(returnSet.Count() == take);
        }

        [Fact]
        public void SeletTop3()
        {
            var store = new EntityStore(_dbConnection);
            int skip = 10, take = 10;

            var lastNameSort = new SortOrderField(typeof(Employee), "LastName");
            var cityNameSort = new SortOrderField(typeof(Employee), "City", SortOrder.Descending);

            var selectQuery = store.GetSelectQuery<Employee>()
                                    .GetTopRows(skip + take)
                                    .Where(e => e.Id > 1)
                                    .OrderBy(lastNameSort, cityNameSort);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);

            var returnSet = employeeSet.Skip(skip).Take(take);

            Xunit.Assert.NotNull(returnSet);
            Xunit.Assert.True(returnSet.Count() == take);
        }

        [Fact]
        public void SelectContains()
        {
            var store = new EntityStore(_dbConnection);
            var employeeIds = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            var selectQuery = store.GetSelectQuery<Employee>()
                                    .Where(e => employeeIds.Contains(e.Id));
                                    

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);

            Xunit.Assert.True(employeeSet.Count() == 7);
        }

        [Fact]
        public void SelectContainsStrings()
        {
            var store = new EntityStore(_dbConnection);
            var employeeLastNames = new List<string> { "Rogers", "Carter", "Daniels" };
            var selectQuery = store.GetSelectQuery<Employee>()
                                    .Where(e => employeeLastNames.Contains(e.LastName));

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());

            var count = employeeSet.ToList().Count();
          
            Xunit.Assert.True(count > 0);
        }

        [Fact]
        public void SelectIntoView()
        {
            var store = new EntityStore(_dbConnection);
            var selectQuery = store.GetSelectQuery<Order>()
                                    .JoinOn<Customer>((o, c) => o.CustomerId == c.Id)
                                    .Where<Order, Customer>((o, c) => c.City == "Gainesville");

            var customerOrders = store.ExecuteQuery<Order, Customer, CustomerOrder>(selectQuery);

            Xunit.Assert.NotNull(customerOrders);

            var custOrder = customerOrders.FirstOrDefault();

            Xunit.Assert.NotNull(custOrder);
            Xunit.Assert.IsType<CustomerOrder>(custOrder);

        }

        [Fact]
        public void SelectWithClassProperties()
        {
            var store = new EntityStore(_dbConnection);
            var loginReq = new LoginRequest();

            loginReq.Username = "srogers";

            var steveRogers = store.GetSingle<Employee>(e => e.Username == loginReq.Username);

            Xunit.Assert.NotNull(steveRogers);

            var margaretCarter = store.GetSingle<Employee>(e => e.Username == TestConstants.USERNAME);

            Xunit.Assert.NotNull(margaretCarter);

            Xunit.Assert.True(margaretCarter.Username.ToLower() == TestConstants.USERNAME.ToLower());
            Xunit.Assert.True(steveRogers.Username.ToLower() == loginReq.Username.ToLower());

        }

        [Fact]
        public void SelectWithNullClassProperty()
        {
            var store = new EntityStore(_dbConnection);
            var loginReq = new LoginRequest();

            loginReq.Username = null;

            var steveRogers = store.GetSingle<Employee>(e => e.Username == loginReq.Username);

            Xunit.Assert.Null(steveRogers);

            var margaretCarter = store.GetSingle<Employee>(e => e.Username == null);

            Xunit.Assert.Null(margaretCarter);
        }

        [Fact]
        public void SelectWithUnsupportedMethodCalls()
        {
            var store = new EntityStore(_dbConnection);
            var loginReq = new LoginRequest();

            loginReq.Username = "srogers";

            NotSupportedException ex = Xunit.Assert.Throws<NotSupportedException>(
                    () => store.GetSingle<Employee>(e => e.Username.Substring(0, 4) == TestConstants.USERNAME));

            var steveRogers = store.GetSingle<Employee>(e => e.Username == loginReq.Username);

            Xunit.Assert.NotNull(steveRogers);
            Xunit.Assert.IsType<Employee>(steveRogers);
        }

        [Fact]
        public void WhereClauseWithDatesAddYears()
        {
            var store = new EntityStore(_dbConnection);
            var singleEmployee = store.GetSingle<Employee>(e => SqlFunctions.AddYears(e.Birthdate, -3) >= DateTime.Now.AddYears(-90)); 

            Xunit.Assert.NotNull(singleEmployee);

            singleEmployee = store.GetSingle<Employee>(e => SqlFunctions.AddYears(e.Birthdate, -3) >= DateTime.Now.AddMonths(-90 * 12));

            Xunit.Assert.NotNull(singleEmployee);

            singleEmployee = store.GetSingle<Employee>(e => SqlFunctions.AddYears(e.Birthdate, -3) >= DateTime.Now.AddDays(-90 * 12 * 30));

            Xunit.Assert.NotNull(singleEmployee);

            singleEmployee = store.GetSingle<Employee>(e => SqlFunctions.AddYears(e.Birthdate, -3) >= DateTime.Now.AddDays(-90 * 12 * 30));

            Xunit.Assert.NotNull(singleEmployee);

            singleEmployee = store.GetSingle<Employee>(e => SqlFunctions.AddYears(e.Birthdate, -3) >= DateTime.Now.AddHours(-90 * 12 * 30 * 24));

            Xunit.Assert.NotNull(singleEmployee);

            singleEmployee = store.GetSingle<Employee>(e => SqlFunctions.AddYears(e.Birthdate, -3) >= DateTime.Now.AddMinutes(-90 * 12 * 30 * 24 * 60));

            Xunit.Assert.NotNull(singleEmployee);
        }

        [Fact]
        public void WhereClauseWithDatesAddMonths()
        {
            var store = new EntityStore(_dbConnection);
            var singleEmployee = store.GetSingle<Employee>(e => SqlFunctions.AddYears(e.Birthdate, -3) >= DateTime.Now.AddMonths(-90));

            Xunit.Assert.NotNull(singleEmployee);
        }

        public void Dispose()
        {
            _dbConnection.Close();
        }
    }
}
