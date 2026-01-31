\ expect: 10
\ Integer square root of 100 via Newton's method
variable sn
: isqrt ( n -- root )
  dup sn !
  dup 2 < if exit then
  2/
  begin
    dup sn @ over / + 2/
    2dup <= if nip exit then
    nip
  0 until ;
: main 100 isqrt . cr ;
