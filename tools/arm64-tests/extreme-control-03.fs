\ expect: 33
\ Test: Multiple EXIT points at different nesting depths
\ Early exit should skip all outer code

: earlyexit ( n -- n )
  dup 10 < if
    drop 33 exit
  then
  dup 20 < if
    drop 66 exit
  then
  drop 99
;

: main
  5 earlyexit
;
