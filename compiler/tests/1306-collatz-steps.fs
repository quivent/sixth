\ expect: 111
variable steps
: collatz ( n -- )
  0 steps !
  begin dup 1 > while
    dup 2 mod 0= if
      2 /
    else
      3 * 1+
    then
    1 steps +!
  repeat drop ;
: main 27 collatz steps @ . cr ;
