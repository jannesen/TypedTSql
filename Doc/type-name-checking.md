# Type/Name checking

## Name checking
Normally, SQL Server checks whether tables, column, etc. names actually exist when used.<br/>
In Typed T-SQL everything is checked at transpile time. So all names must resolve during transpile time. This prevents many errors during run time.


## Type checking
All expressions and assignments are type checked (in the transpiler). The prevents many errors during runtime.

Typed T-SQL has 4 different levels of type checking:

| name       | description                                                  |
| ---------- | ------------------------------------------------------------ |
| **T-Sql**  | Standard Transact-SQL                                        |
| **Safe**   | Only safe conversion are allowed. Like as smallint to int, varchar (30) to varchar (31), etc.<br/>Conversion from int to smallint is not allowed. |
| **Strong** | No native type conversion allowed.<br/>Conversion between type alias and native-type are allowed. Conversion between different type alias with the same native type are allowed. |
| **Strict** | No conversion is allowed. Not between type aliases and or native type. |


###  sample strict
```SQL
TYPE [relation_id] FROM INT WITH TYPECHECK STRICT
TYPE [person_id] FROM INT WITH TYPECHECK STRICT

DECLARE @i1 INT
DECLARE @r1 [relation_id]
DECLARE @r2 [relation_id]
DECLARE @p1 [person_id]

SET @r1 = @r2 -- allowed
SET @p1 = @r2 -- error
SET @p1 = @i1 -- error
```