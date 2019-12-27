# Typed T-Sql
Typed T-Sql is a programming language and extension for Visual Studio 2019.
The programming language is based on Transact-SQL and is intended for programming stored procedures, functions, trigger and view.

For programming a some procedures, functions, etc. Microsoft SQL Server Management Studio is okay.
But for an application with hundreds of procedures, functions, etc., this becomes unmanageable. 
There is where Typed T-Sql shines. It allows programs SQL Server in a way you are used with modern IDE.
Typed T-Sql is a transpiler that generates T-SQL. There is no runtime component. The output is a standard T-SQL script.
Installing the code in a production environment is just running sql script.

Typed T-Sql adds the following:
* [static name check](Doc/type-name-checking.md)
* [static type check](Doc/type-name-checking.md).
* [constants](Doc/decl/type.md)
* [automatic variable declaration by using var](Doc/var-let.md)
* [DROP ... IF EXISTS is not necessary](Doc/sample/hello-world.md)
* [simply assigning rights to the procedures, functions.](Doc/sample/hello-world.md)

Statement extensions:
* [for select loop](Doc/stat/for-select.md)
* [exec_sql](Doc/stat/exec_sql.md)
* [named insert](Doc/stat/named-insert.md)
* [store](Doc/stat/store.md)

Function extensions:
* [filebinary](Doc/func/filebinary.md)
* [is_equal](Doc/func/is_equal.md)
* [is_not_equal](Doc/func/is_equal.md)
* [openjson](Doc/func/openjson.md)
* [json_value](Doc/func/json_value.md)

IDE function:
* [catalog browser](Doc/ide/.md)
* [quick info](Doc/ide/quick-info.md)
* [find all references](Doc/ide/find-all-references.md)
* [refactoring](Doc/ide/refactoring.md)

## webservice extension

This extensions which must be explicit loaded.<br/>
It works together with Jannesen.Web and jc3. Allow the creation to web api method to be called by http. It can also create proxy that can be used in jc3 and give end to end name/type checking.

It implements the following declarations:

* [WEBSERVICE](Doc/webservice/webservice.md)
* [WEBMETHOD](Doc/webservice/webmethod.md)
* [WEBCOMPLEXTYPE](Doc/webservice/webcomplextype.md)