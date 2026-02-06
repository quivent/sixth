\ expect: 0
\ Test: Deep loop nesting with memory operations
\ Stress test memory access with nested loops

variable data
variable ok

: setup here data ! 128 allot ;

: write-seq ( -- )
  65 0 do
    i data @ i + c!
  loop ;

: read-seq ( -- flag )
  1 ok !
  65 0 do
    data @ i + c@ i <> if 0 ok ! then
  loop ok @ ;

: scramble ( -- )
  32 0 do
    data @ i + c@
    data @ 63 i - + c@
    data @ i + c!
    data @ 63 i - + c!
  loop ;

: verify-scr ( -- flag )
  1 ok !
  32 0 do
    data @ i + c@ 63 i - <> if 0 ok ! then
    data @ 63 i - + c@ i <> if 0 ok ! then
  loop ok @ ;

: main
  setup
  write-seq
  read-seq 0= if 1 exit then
  scramble
  verify-scr 0= if 2 exit then
  0 ;
