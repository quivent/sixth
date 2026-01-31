\ expect: 10
\ Test: value under loop counter survives → 10
: main 10 3 begin dup 0 > while 1- repeat drop . cr ;
