\ expect: 1 2 3 1 2 3 1 2 3
: counted ( n -- )
  dup 0= if drop exit then
  4 1 do i . loop
  1- recurse
;
: main 3 counted cr ;
