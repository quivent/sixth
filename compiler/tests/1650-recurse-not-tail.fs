\ expect: 120
: fact ( n -- n! ) dup 1 > if dup 1- recurse * else drop 1 then ;
: main 5 fact . cr ;
