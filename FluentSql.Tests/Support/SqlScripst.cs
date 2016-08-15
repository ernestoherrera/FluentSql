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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
DROP TABLE [dbo].[Employees]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND type in (N'U'))
DROP TABLE [dbo].[Customers]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
DROP TABLE [dbo].[Orders]


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
	[CustomerId] [nchar](5) NULL,
	[EmployeeId] [int] NULL,
	[OrderDate] [datetime] NULL,
	[RequiredDate] [datetime] NULL,
	[ShippedDate] [datetime] NULL,
	[ShipVia] [int] NULL,
	[Freight] [money] NULL,
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

INSERT INTO Employees (FirstName,LastName,Email,[Address],City,[State],[SSN])
VALUES

('Steve','Rogers','srogers@bloglines.com','5 Schlimgen Lane','Mobile','AL','443-89-5912'),
('Nathan','Spencer','nspencer1@msn.com','4 Waubesa Pass','Sarasota','FL','100-96-5760'),
('Margaret','Carter','mcarter2@marvel.com','93290 Blue Bill Park Park','Jacksonville','FL','511-45-0763'),
('Joan','Daniels','jdaniels3@moonfruit.com','92 Paget Trail','Kansas City','MN','657-24-5146'),
('Brenda','Stone','bstone4@oakley.com','05 Pennsylvania Pass','Humble','TX','945-17-1532');



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
", TestConstants.TestDatabaseName);
            }
        }
     }
 }