# WEBMETHOD
## syntax
```
WEBMETHOD webservicename::'name'
    METHOD 'method'
    (HANDLER 'handler' | PROXY 'proxymodule:name' )
    [ optionname = 'value'[,...] ]
    [ DEPENDENTASSEMBLY 'assemblyname'[,...] ]
    [ HANDLERCONFIG `XML[ xmlisland ]LMX` ]
(
    <parameter>[,...]
)
[ WITH <procedure_option>]
[ GRANT groupname[,...]]
BEGIN
    statements
END

<parameter> ::=
    @name SOURCE 'source'
        [ ( <customsource>[,...] )]
        [ REQUIRED | DEFAULT = defaultvalue ]
        [ AS `[ module:typescript-rich-datatype ]` ]

<procedure_option> ::=
    [ ENCRYPTION ]  
    [ RECOMPILE ]  
    [ EXECUTE AS Clause ]  

```


##### webservicename
name of [webservice](webservice.md) (defined by webservice) 

##### name
name of the web method

##### method
http method for example 'GET'|'POST'|'DELETE'

##### handler
the name of the http handler. for example sql-json2

##### proxymodule:name
proxymodule, name of the proxy inthe module.

##### optionname = 'value'
option passed tru to http handler

##### assemblyname
assembly that must be loaded by jannesen.web

##### xmlisland
xmlisland passed tru to http handler