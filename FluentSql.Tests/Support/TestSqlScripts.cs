using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Tests.Support
{
    internal class TestSqlScripts
    {
        public static string TEST_DB_NAME = "FluentSqlTestDb";
        public static string CREATE_DATABASE = $@"
IF NOT EXISTS (SELECT TOP 1 * FROM master.dbo.sysdatabases o where ( o.name  = '{TEST_DB_NAME}'))
    CREATE DATABASE {TEST_DB_NAME};

IF(EXISTS (SELECT 1 FROM [{TEST_DB_NAME}].INFORMATION_SCHEMA.TABLES T WHERE T.TABLE_NAME = 'Person'))
    DROP TABLE [{TEST_DB_NAME}].[dbo].[Person];

IF(EXISTS (SELECT 1 FROM [DAPPERSQLTEST].INFORMATION_SCHEMA.TABLES T WHERE T.TABLE_NAME = 'User'))
    DROP TABLE [{TEST_DB_NAME}].[dbo].[User];";

        public static string CREATE_TABLES = $@"
CREATE TABLE [{TEST_DB_NAME}].[dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[ExternalKey] [varchar](155) NULL,
	[Username] [varchar](50) NULL,
	[Password] [varchar](50) NULL,
	[ApiKey] [varchar](250) NULL,
	[Email] [varchar](250) NULL,
	[Enabled] [bit] NULL,
	[LastLogin] [datetime] NULL CONSTRAINT [DF_Users_LastLogin]  DEFAULT (getdate()),
	[ForceReset] [bit] NOT NULL CONSTRAINT [DF_Users_ForceReset]  DEFAULT ((0)),
	[IPSecurityMask] [varchar](20) NULL,
	[RoleMask] [bigint] NULL,
	[Deleted] [bit] NOT NULL CONSTRAINT [DF_Users_Deleted]  DEFAULT ((0)),
	[DeletedDate] [datetime] NULL,
	[LastLogout] [datetime] NULL,
	[AccessKey] [varchar](128) NULL,
	[SecretKey] [varchar](128) NULL,
	[PasswordExpiration] [date] NULL,	
	[ModifiedBy] [int] NULL,
	[Modified] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Users_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Users_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [{TEST_DB_NAME}].[dbo].[Person](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ExternalKey] [varchar](155) NULL,
	[FirstName] [varchar](255) NULL,
	[MiddleName] [varchar](255) NULL,
	[LastName] [varchar](255) NULL,
	[Suffix] [varchar](255) NULL,
	[Title] [varchar](255) NULL,
	[Company] [varchar](255) NULL,
	[Tags] [varchar](255) NULL,
	[ExtraInfo] [text] NULL,
	[Active] [bit] NOT NULL CONSTRAINT [DF_Persons_Active]  DEFAULT ((1)),
	[Type] [int] NOT NULL CONSTRAINT [DF_Persons_Type]  DEFAULT ((0)),
	[Enabled] [bit] NOT NULL CONSTRAINT [DF_Persons_Enabled]  DEFAULT ((1)),
	[FacebookUserId] [varchar](50) NULL,
	[LinkedInUserId] [varchar](50) NULL,
	[Birthday] [datetime] NULL,
	[Anniversary] [datetime] NULL,
	[TwitterUserId] [varchar](50) NULL,
	[OkayToContact] [bit] NOT NULL CONSTRAINT [DF_Persons_OkayToContact]  DEFAULT ((1)),
	[ModifiedBy] [int] NULL,
	[Modified] [datetime] NULL CONSTRAINT [DF_Persons_Modified]  DEFAULT (getdate()),
	[CreatedBy] [int] NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Persons_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Persons] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

DECLARE @insertedIds TABLE (Id INT);
DECLARE @Id INT = 0;

INSERT INTO PERSON ( FirstName, LastName, Company)
OUTPUT inserted.Id INTO @insertedIds
VALUES ('Tony', 'Stark', 'Stark Enterprises');
SET @Id = (SELECT TOP 1 Id FROM @insertedIds);
INSERT INTO [User] (PersonId, UserName, Email)
VALUES (@Id, 'TSTARK', 'tony.stark@starkenterprises')
";

    }
}
