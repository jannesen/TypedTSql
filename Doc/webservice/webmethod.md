# WEBMETHOD

## syntax
```
WEBMETHOD webservicename::'name'
    METHOD 'method'
    (PROXY 'proxymodule:name' | 
       ( HANDLER 'handler'
         [ optionname = 'value'[,...] ]
         [ DEPENDENTASSEMBLY 'assemblyname'[,...] ]
         [ HANDLERCONFIG `XML[ xmlisland ]LMX` ] ))
(
    <parameter>[,...]
    
    [ <RETURNS statement> ]
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

## description

| <name> | description |
|------------------------------|------------------------------------------|
| webservicename | name of webservice (see [webservice](webservice.md)) |
| name | name of the web method |
| method | http method for example 'GET' |
| proxymodule:name | Define a json webapi.<br />create a proxy in typescript for easy calling and end to end typechecking<br />**proxymodule** the typescript module.<br />**name** of the proxy in the module. |
| RETURNS statement | The data that is passed back in de response body (see [returns)](webreturns.md). |
| source | Source of de parameter. can be querystring, textjson, etc. <br />See jannesen.web for possible values |
| handler | the name of the http handler.<br />See jannesen.web handlers for possible handlers |
| optionname = 'value' | option passed tru to http handler.<br />See jannesen.web handlers for possible options. |
| assemblyname | assembly that must be loaded by jannesen.web |
| xmlisland | xmlisland passed tru to http handler<br />See handler for further information |

