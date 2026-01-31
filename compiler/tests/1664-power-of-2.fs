\ expect: 1 0 1 0
\ Check if number is a power of 2: n>0 and (n & n-1) == 0
: pow2? ( n -- 0|1 )
  dup 0> if
    dup 1- and 0= if 1 else 0 then
  else
    drop 0
  then ;
: main
  64 pow2? . 48 pow2? . 256 pow2? . 0 pow2? . cr ;
