\ Test 727: recursive pure word whose result is used
: fact dup 1 > if dup 1- recurse * else drop 1 then ;
: main 5 fact . cr ;
