using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Tests.SqlScripts.Postgres
{
    internal class PostgresSqlScripts
    {
        public PostgresSqlScripts()
        { }

        public static string CREATE_TABLES
        {
            get
            {
                return string.Format(
@"
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
	
 	CONSTRAINT pk_employee_Id PRIMARY KEY(	Id))



");

            }

        }
    }
}
