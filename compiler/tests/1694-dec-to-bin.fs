\ expect: 11010
\ Convert 26 to binary string output
\ 26 = 11010
create bits 16 allot
variable bcount
: dec2bin ( n -- )
  0 bcount !
  begin dup 0> while
    dup 1 and 48 + bcount @ bits + c!
    bcount @ 1+ bcount !
    1 rshift
  repeat drop
  \ print in reverse
  bcount @ 0 do
    bcount @ 1- i - bits + c@ emit
  loop ;
: main 26 dec2bin cr ;
