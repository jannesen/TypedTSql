# Typed T-Sql
Typed T-Sql is a programming language and extension for Visual Studio 2019.
The programming language is based on Transact-SQL and is intended for programming stored procedures, functions and view.

For programming a some procedures, functions, etc Microsoft SQL Server Management Studio is oke.
But for an application with hundreds of procedures, functions, etc., this becomes unmanageble. 
There is where Typed T-Sql shines. It programs SQL Server as you are used to with modern IDE.
Typed T-Sql is a transpiler that generates T-SQL.
Installing the code in a production environment is just running sql script.

Typed T-Sql adds the following:

* DROP ... IF EXISTS is no longer necessary..
* simply assigning rights to the procedures, functions.
* static checking of names (table,column,etc).
* static type check.
* constants
* Language extensions
* quick info
* find all references
* refactoring

## DROP ... IF EXISTS meer nodig / rechten

T-SQL:
```
DROP PROCEDDURE IF EXISTS dbo.hello_world
GO
CREATE PROCEDURE dbo.hello_world
AS
BEGIN
    SELECT 'Hello world'
END
GO
GRANT EXECUTE dbo.hello_world TO public
GO
```

Typed T-SQL
```
PROCEDURE hello_world
GRANT EXECUTE TO public
BEGIN
    SELECT 'Hello world'
END
```


## statische controlle van namen (table,column,etc).
Normally, SQL Server checks whether tables, column, etc. actually exist with the first use of the code.<br/>
In Typed T-SQL everything is checked transpile time. This prevents many errors during run time.


## statische type controle
SQL Server automatically converts between the different types.
Typed T-SQL has 4 different levels of type checking:

* **T-Sql**<br/>
  Standard Transact-SQL
* **Safe**<br/>
  Only safe conversion are allowed. Like as smallint to int, varchar (30) to varchar (31), etc. Conversion from int to smallint is not allowed.
* **Strong**<br/>
  No native type conversion allowed. Conversion between type alias with the same native type is allowed.
* **Strict**<br/>
  No conversion is allowed. Also not between type alias with the same native type.
  
SQL Server has [Type alias](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-type-transact-sql).
This type of alias can also be defined and used in Typed T-SQL.
Sample:
```
TYPE [relation_id] FROM INT WITH TYPECHECK STRICT
TYPE [person_id] FROM INT WITH TYPECHECK STRICT

DECLARE @r1 [relation_id]
DECLARE @r2 [relation_id]
DECLARE @p1 [person_id]

SET @r1 = @r2 -- Toegestaan
SET @p1 = @r2 -- Niet toegestaan
```


## constanten
Typed T-SQL supports define constant. Sample:
```
TYPE [t]
FROM INT
WITH TYPECHECK STRICT
VALUES (
	[a] = 1,
	[b] = 2,
	[c] = 3
)

DECLARE @i [t]

SET @i = [t]::[b] -- transpiled to SET @i = 2
```


## taal extensies
Some language extensions:
* IS_EQUAL(a,b)<br/>
  transpile to ((a = b) or (a is null and b is null))
* IS_NOT_EQUAL(a,b)<br/>
  transpile to ((a <> b) or (a is null and b is not null) or (a is not null and b is null))
* WEBSERVICE,WEBMETHOD (extensies welke expliet geladen moet worden)<br/>
  together with Jannesen.Web and jc3, ajax calls in the browser are linked to code (stored procedure) in sql server.


## quick info
Move the mouse cursor over a symbol and a quick info popup appears with useful information.


## find all references
Similar to https://docs.microsoft.com/en-us/visualstudio/ide/finding-references but for Typed T-SQL


## refactoring
The Typed T-SQL extension in visual studio supports refactoring. Including refactoring of tables, column, etc.<br/>
In the case of a table, column refactoring, extended_properties are added so that [DBTools](https://github.com/jannesen/DBTools) can follow the refactoring when generating a patch script.
