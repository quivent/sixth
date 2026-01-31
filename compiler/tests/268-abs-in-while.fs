\ expect: 0
\ Test: counting up from negative with 0< check → 0
: main -5 begin dup 0< while 1+ repeat . cr ;
