# WEBSERVICE
## syntax
```
WEBSERVICE name 
[ EMITBASEPATH='emitbasepath' ]
[ DATABASE='database' ]
[ BASEURL='baseurl' ]
[ INDEX='indexservice' ]
[ TYPEMAP (
    <typemapentry>[,...]
)]

<typemapentry> ::=
    udtname AS `[ module:typescript-rich-datatype ]`
```

##### emitdatabasepath
The path where de jannesen.web.config and proxy.ts file are emited.

##### database
The database resource that is use in the jannesen.web.config http-handlers

##### baseurl
The baseurl for the module that are references.

##### indexservice
The nameof a service call that e array or string returns with alle webservice call that the user has access to.