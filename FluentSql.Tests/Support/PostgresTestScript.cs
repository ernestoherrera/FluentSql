using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Tests.Support
{
    internal class PostgresTestScript
    {
        public static string CREATE_DATABASE_OBJECTS
        {
            get
            {
                return @"
DO
$do$
BEGIN

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'vwcustomerorders' and table_schema = 'public') THEN
	DROP VIEW vwcustomerorders;
END IF;

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'employees' and table_schema = 'public') THEN
	DROP TABLE Employees;
END IF;

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'customers' and table_schema = 'public') THEN
	DROP TABLE Customers;
END IF;
    
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'orderdetails' and table_schema = 'public') THEN
	DROP TABLE OrderDetails;
END IF;

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'orders' and table_schema = 'public') THEN
    DROP TABLE Orders;
END IF;

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'products' and table_schema = 'public') THEN   
	DROP TABLE Products;
END IF;

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'employees' and table_schema = 'public') THEN

    CREATE TABLE Employees (
        Id SERIAL,
        Username  varchar(150) NOT NULL,
        FirstName varchar(150) NULL,
        LastName varchar(150) NULL,
        Email varchar(250) NULL,
        Password varchar(126) NULL,
        ApiKey varchar(256) NULL,
        Enabled boolean NULL CONSTRAINT df_employee_enabled DEFAULT(TRUE),
        Birthdate Date null,
        Address varchar(512) NULL,
        City varchar(30) NULL,
        State varchar(5) NULL,
        SSN varchar(20) NULL,
        Created timestamp NULL CONSTRAINT df_employee_created default(now()),

        CONSTRAINT pk_employee_Id PRIMARY KEY(	Id)
    );

END IF;

IF  NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'customers' and table_schema = 'public') THEN

	CREATE TABLE Customers(
	Id SERIAL,
	CompanyName varchar(40) NOT NULL,
	ContactName varchar(30) NULL,
	ContactTitle varchar(30) NULL,
	Address varchar(60) NULL,
	City varchar(15) NULL,
	Region varchar(15) NULL,
	PostalCode varchar(10) NULL,
	Country varchar(15) NULL,
	Phone varchar(24) NULL,
	Fax varchar(24) NULL,
 	CONSTRAINT pk_customers_id PRIMARY KEY ( Id )
    );
END IF;

IF  NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'orderdetails' and table_schema = 'public') THEN
	CREATE TABLE OrderDetails(
	OrderId int NOT NULL,
	ProductId int NOT NULL,
	UnitPrice money NOT NULL,
	Quantity smallint NOT NULL,
	Discount real NOT NULL,
	CONSTRAINT PK_Order_Details PRIMARY KEY ( OrderId , ProductId )
	);
END IF;

IF  NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'orders' and table_schema = 'public') THEN
	CREATE TABLE Orders(
	Id serial NOT NULL,
	CustomerId int NULL,
	EmployeeId int NULL,
	OrderDate timestamp with time zone NULL,
	RequiredDate timestamp with time zone NULL,
	ShippedDate timestamp with time zone NULL,
	ShipVia int NULL,
	Freight money NULL,
	DaysSinceOrdered  int NULL,
	ShipName varchar(40) NULL,
	ShipAddress varchar(60) NULL,
	ShipCity varchar(15) NULL,
	ShipRegion varchar(15) NULL,
	ShipPostalCode varchar(10) NULL,
	ShipCountry varchar(15) NULL,
 	CONSTRAINT PK_Orders PRIMARY KEY( Id )
	);
END IF;

IF  NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'products' and table_schema = 'public') THEN
	
    CREATE TABLE Products(
	ProductId serial NOT NULL,
	ProductName varchar(40) NOT NULL,
	QuantityPerUnit varchar(20) NULL,
	UnitPrice money NULL,
	UnitsInStock smallint NULL,
	UnitsOnOrder smallint NULL,
	ReorderLevel smallint NULL,
	Discontinued boolean NOT NULL,
	CONSTRAINT PK_Products PRIMARY KEY (ProductId)
	);
END IF;

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE LOWER(table_name) = 'vwcustomerorders' and table_schema = 'public') THEN

    CREATE VIEW vwCustomerOrders
    AS
    SELECT o.Id, o.CustomerId, o.EmployeeId, o.OrderDate, o.RequiredDate, 
        o.ShippedDate, o.ShipVia, o.Freight, o.ShipName, o.ShipAddress, o.ShipCity, 
        o.ShipRegion, o.ShipPostalCode, o.ShipCountry, 
        c.CompanyName, c.Address, c.City, c.Region, c.PostalCode, c.Country
    FROM Customers c JOIN Orders o ON c.Id = o.CustomerId;

END IF;


INSERT INTO Products(ProductId,ProductName,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued) VALUES(1,'Chai','10 boxes x 20 bags',18,39,0,10,FALSE);
INSERT INTO Products(ProductId,ProductName,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued) VALUES(2,'Chang','24 - 12 oz bottles',19,17,40,25,FALSE);
INSERT INTO Products(ProductId,ProductName,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued) VALUES(3,'Aniseed Syrup','12 - 550 ml bottles',10,13,70,25,FALSE);
INSERT INTO Products(ProductId,ProductName,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued) VALUES(4,'Chef Anton''s Cajun Seasoning','48 - 6 oz jars',22,53,0,0,FALSE);

INSERT INTO Employees (FirstName, LastName, Email, Address, City, State, SSN)
VALUES

('Steve','Rogers','srogers@bloglines.com','5 Schlimgen Lane','Mobile','AL','443-89-5912'),
('Nathan','Spencer','nspencer1@msn.com','4 Waubesa Pass','Sarasota','FL','100-96-5760'),
('Margaret','Carter','mcarter2@marvel.com','93290 Blue Bill Park Park','Jacksonville','FL','511-45-0763'),
('Joan','Daniels','jdaniels3@moonfruit.com','92 Paget Trail','Kansas City','MN','657-24-5146'),
('Brenda','Stone','bstone4@oakley.com','05 Pennsylvania Pass','Humble','TX','945-17-1532'),
('Terry','Mcdonald','tmcdonald17@jalbum.net','9 Anderson Street','Salt Lake City','UT','976-98-7782'),
('Mary','Gomez','mgomez18@dell.com','303 Lotheville Parkway','WA','DC','131-44-2595'),
('Nicholas','Freeman','nfreeman19@hatena.ne.jp','79604 Oriole Court','Portland','OR','310-12-8117'),
('Andrew','Scott','ascott1a@businessweek.com','677 Buhler Hill','Montgomery','AL','553-46-4720'),
('Matthew','Hawkins','mhawkins1b@prweb.com','74 Sutherland Road','Houston','TX','427-33-6377'),
('Angela','Simpson','asimpson1c@seattletimes.com','87 Pleasure Park','London','KY','138-89-8270'),
('Jacqueline','West','jwest1d@disqus.com','34 Thackeray Point','Mobile','AL','853-32-2479'),
('John','Barnes','jbarnes1e@loc.gov','49754 Ilene Road','Saint Paul','MN','744-85-1423'),
('David','Simpson','dsimpson1f@moonfruit.com','51542 Kenwood Junction','WA','DC','606-81-7094'),
('Timothy','Mcdonald','tmcdonald1g@over-blog.com','2 Grim Court','Knoxville','TN','670-71-1926'),
('Evelyn','Hunter','ehunter1h@cornell.edu','74190 Sycamore Circle','Rochester','NY','908-73-5738'),
('Bobby','Peterson','bpeterson1i@ustream.tv','73224 Jenna Terrace','South Bend','IN','912-85-4444'),
('Daniel','Ross','dross1j@answers.com','32 Autumn Leaf Street','Richmond','VA','139-75-6918'),
('Shirley','Barnes','sbarnes1k@businesswire.com','1 Laurel Place','Anchorage','AK','602-09-6724'),
('Roger','Robertson','rrobertson1l@mashable.com','583 Caliangt Hill','Aurora','IL','596-22-3201'),
('Janice','Rivera','jrivera1m@uol.com.br','8185 Fordem Park','Fort Lauderdale','FL','368-17-5248'),
('Joseph','Peterson','jpeterson1n@usnews.com','47 Brickson Park Way','WA','DC','993-51-6711'),
('Stephanie','Cole','scole1o@washingtonpost.com','235 Almo Junction','Fort Worth','TX','268-77-7654'),
('John','Cruz','jcruz1p@ehow.com','79 Victoria Hill','WA','DC','857-12-0241'),
('Kelly','Vasquez','kvasquez1q@senate.gov','66 Lakeland Terrace','El Paso','TX','643-41-9926'),
('Keith','Alexander','kalexander1r@google.nl','3037 La Follette Circle','Santa Monica','CA','809-99-0328'),
('Dorothy','Sanchez','dsanchez1s@go.com','2387 John Wall Way','Spokane','WA','787-28-7723');
    
END
$do$ 
";
            }
        }
    }
}
