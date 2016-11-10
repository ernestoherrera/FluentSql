using System;
 using System.Collections.Generic;
 using System.Linq;
 using System.Text;
 using System.Threading.Tasks;
 
 namespace FluentSql.Tests.Support
 {
     internal class SqlScripts
     {
         public static string CREATE_DATABASE
        {
            get
            {
                return string.Format(
  @" IF NOT EXISTS (SELECT TOP 1 * FROM master.dbo.sysdatabases o where ( o.name  = '{0}'))
     CREATE DATABASE {0};", TestConstants.TestDatabaseName);
            }
        }

        public static string CREATE_TABLES { get
            {
                return string.Format(
    @"USE {0};

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwCustomerOrders]'))
DROP VIEW [dbo].[vwCustomerOrders]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
DROP TABLE [dbo].[Employees]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND type in (N'U'))
DROP TABLE [dbo].[Customers]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
DROP TABLE [dbo].[Orders]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderDetails]') AND type in (N'U'))
DROP TABLE [dbo].[OrderDetails]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
DROP TABLE [dbo].[Products]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].Employees (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username]  as substring(FirstName, 1, 1) + LastName,
	[FirstName] [varchar](150) NULL,
	[LastName] [varchar](150) NULL,
	[Email] [varchar](250) NULL,
	[Password] [varchar](126) NULL,
	[ApiKey] [varchar](256) NULL,
	[Enabled] bit NULL CONSTRAINT [df_employee_enabled] DEFAULT(1),
	[Birthdate] Date null,
	[Address] [varchar](512) NULL,
	[City] [varchar](30) NULL,
	[State] [varchar](5) NULL,
	[SSN] [varchar](20) NULL,
	[Created] DateTime NULL CONSTRAINT [df_employee_created] default(getdate()),
	[Photo] [image]
 CONSTRAINT [pk_employee_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
  
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Customers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyName] [nvarchar](40) NOT NULL,
	[ContactName] [nvarchar](30) NULL,
	[ContactTitle] [nvarchar](30) NULL,
	[Address] [nvarchar](60) NULL,
	[City] [nvarchar](15) NULL,
	[Region] [nvarchar](15) NULL,
	[PostalCode] [nvarchar](10) NULL,
	[Country] [nvarchar](15) NULL,
	[Phone] [nvarchar](24) NULL,
	[Fax] [nvarchar](24) NULL,
 CONSTRAINT [pk_customers_id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Orders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] int NULL,
	[EmployeeId] [int] NULL,
	[OrderDate] [datetime] NULL,
	[RequiredDate] [datetime] NULL,
	[ShippedDate] [datetime] NULL,
	[ShipVia] [int] NULL,
	[Freight] [money] NULL,
	[DaysSinceOrdered]  AS (DATEDIFF(day,case when [OrderDate] IS NULL then getdate() else [OrderDate]  end,case when [REQUIREDDATE] IS NULL then getdate() else [REQUIREDDATE] end)),
	[ShipName] [nvarchar](40) NULL,
	[ShipAddress] [nvarchar](60) NULL,
	[ShipCity] [nvarchar](15) NULL,
	[ShipRegion] [nvarchar](15) NULL,
	[ShipPostalCode] [nvarchar](10) NULL,
	[ShipCountry] [nvarchar](15) NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END

CREATE TABLE [dbo].[Products](
	[ProductId] [int] IDENTITY(1,1) NOT NULL,
	[ProductName] [nvarchar](40) NOT NULL,
	[QuantityPerUnit] [nvarchar](20) NULL,
	[UnitPrice] [decimal](10,2) NULL,
	[UnitsInStock] [smallint] NULL,
	[UnitsOnOrder] [smallint] NULL,
	[ReorderLevel] [smallint] NULL,
	[Discontinued] [bit] NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[OrderDetails](
	[OrderId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[UnitPrice] [decimal](10,2) NOT NULL,
	[Quantity] [smallint] NOT NULL,
	[Discount] [real] NOT NULL,
 CONSTRAINT [PK_Order_Details] PRIMARY KEY CLUSTERED 
(
	[OrderId] ASC,
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwCustomerOrders]'))
EXEC dbo.sp_executesql @statement = N'
CREATE VIEW [dbo].[vwCustomerOrders] AS
SELECT o.Id, o.CustomerId, o.EmployeeId, o.OrderDate, o.RequiredDate, 
	o.ShippedDate, o.ShipVia, o.Freight, o.ShipName, o.ShipAddress, o.ShipCity, 
	o.ShipRegion, o.ShipPostalCode, o.ShipCountry, 
	c.CompanyName, c.Address, c.City, c.Region, c.PostalCode, c.Country
FROM Customers c JOIN Orders o ON c.Id = o.CustomerId
'

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustOrderHist]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[CustOrderHist] @CustomerID nchar(5)
AS
SELECT ProductName, Total=SUM(Quantity)
FROM Products P, [OrderDetails] OD, Orders O, Customers C
WHERE C.Id = @CustomerID
AND C.Id = O.CustomerID AND O.Id = OD.OrderID AND OD.ProductID = P.ProductID
GROUP BY ProductName
' 
END

SET IDENTITY_iNSERT PRODUCTS ON

INSERT Products(ProductId,ProductName,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued) VALUES(1,'Chai','10 boxes x 20 bags',18,39,0,10,0);
INSERT Products(ProductId,ProductName,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued) VALUES(2,'Chang','24 - 12 oz bottles',19,17,40,25,0);
INSERT Products(ProductId,ProductName,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued) VALUES(3,'Aniseed Syrup','12 - 550 ml bottles',10,13,70,25,0);
INSERT Products(ProductId,ProductName,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued) VALUES(4,'Chef Anton''s Cajun Seasoning','48 - 6 oz jars',22,53,0,0,0);


INSERT INTO Employees (FirstName,LastName,Email,[Address],City,[State],[SSN])
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

INSERT [Customers] VALUES('Romano''s Macaroni Grill','Joseph Smith','Sales Representative','6419 W Newberry Rd','Gainesville',NULL,'32606','USA','(352) 331-0637','');
INSERT [Customers] VALUES('Leonardo''s Pizza','Leonardo Daglio','Owner','1245 W University Ave','Gainesville',NULL,'32601','USA','(352) 375-2008','');

INSERT INTO [Orders] (CustomerId,EmployeeId,OrderDate,RequiredDate,
	ShippedDate,ShipVia,Freight,ShipName,ShipAddress,
	ShipCity,ShipRegion,ShipPostalCode,ShipCountry)
VALUES (1,1,'1/17/2016','2/14/2016','2/23/2016',1,140.51,
	N'Macarroni Grill',N'Queen Elizabeth 34th street',N'Gainesville',
	NULL,N'32601',N'USA');

INSERT INTO [Orders] (CustomerId,EmployeeID,OrderDate,RequiredDate,
	ShippedDate,ShipVia,Freight,ShipName,ShipAddress,
	ShipCity,ShipRegion,ShipPostalCode,ShipCountry)
VALUES (1,4,'1/18/2016','2/15/2016','2/25/2016',3,3.25,
	N'Mexico Centre',N'345 Toledo Drive',N'New México',
	NULL,N'456878',N'USA');
INSERT INTO [Orders] (CustomerId,EmployeeID,OrderDate,RequiredDate,
	ShippedDate,ShipVia,Freight,ShipName,ShipAddress,
	ShipCity,ShipRegion,ShipPostalCode,ShipCountry)
VALUES (2,1,'1/19/2016','2/16/2016','2/29/2016',1,55.09,
	N'Prince of York',N'Henry VIII drive',N'London',
	NULL,N'50739',N'England');

  INSERT OrderDetails VALUES (1, 1, 3.33, 7, 0.0);
  INSERT OrderDetails VALUES (1, 2, 17.33, 20, 0.0);

  INSERT OrderDetails VALUES (2, 3, 45.81, 8, 0.0);
  INSERT OrderDetails VALUES (2, 4, 9.99, 100, 0.0);

  INSERT OrderDetails VALUES (3, 1, 3.33, 100, 0.0);
  INSERT OrderDetails VALUES (3, 2, 17.99, 50, 0.0);
  INSERT OrderDetails VALUES (3, 3, 45.99, 2, 0.0);
", TestConstants.TestDatabaseName);
            }
        }
     }
 }