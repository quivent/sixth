\ expect: even odd even odd even
: parity ( n -- )
  2 mod 0= if s" even" type else s" odd" type then
  32 emit
;
: main
  5 0 do
    i parity
  loop
  cr
;
