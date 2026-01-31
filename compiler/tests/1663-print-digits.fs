\ expect: 4 2 7
\ Print each digit of 427 from most significant
create digits 80 allot
variable dcount
: split ( n -- )
  0 dcount !
  begin dup 0> while
    dup 10 mod
    dcount @ cells digits + !
    dcount @ 1+ dcount !
    10 /
  repeat drop ;
: main
  427 split
  dcount @ 0 do
    dcount @ 1- i - cells digits + @ .
  loop cr ;
