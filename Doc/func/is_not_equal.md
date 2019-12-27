# IS_EQUAL

Compare if 2 values are not equal. Including NULL.

## syntax
```
IS_NOT_EQUAL(a,b)
```

## description
true is a !== b. If a is null and b is not null then also true. 

## transpile
```
((a <> b) or (a is null and b is not null) or (a is not null and b is null))
```
