# IS_EQUAL

Compare if 2 values are equal. Including NULL.

## syntax
```
IS_EQUAL(a,b)
```

## description
true is a === b. If a is null and b is null then also true. 

## transpile
```
((a = b) or (a is null and b is null))
```
