\ expect: 0
\ Test: Unsigned comparison U< and U>
\ These treat the top bit as magnitude, not sign

: min-int  1 63 lshift ;                    \ $8000000000000000
: max-uint -1 ;                             \ $FFFFFFFFFFFFFFFF (all bits set)

: main
  \ Unsigned: -1 is MAX-UINT, so it's > any positive number
  0 -1 u< -1 <> if 1 exit then              \ 0 U< MAX-UINT
  -1 0 u> -1 <> if 2 exit then              \ MAX-UINT U> 0

  \ Unsigned: MIN-INT is $8000..., which is > MAX-INT ($7FFF...)
  min-int 0 u> -1 <> if 3 exit then         \ $8000... U> 0
  min-int 1 u> -1 <> if 4 exit then         \ $8000... U> 1

  \ Compare two large unsigned values
  -1 -2 u> -1 <> if 5 exit then             \ $FFFF... U> $FFFE...
  -2 -1 u< -1 <> if 6 exit then             \ $FFFE... U< $FFFF...

  \ Equal values
  -1 -1 u< 0 <> if 7 exit then              \ NOT (MAX-UINT U< MAX-UINT)
  -1 -1 u> 0 <> if 8 exit then              \ NOT (MAX-UINT U> MAX-UINT)

  0
;
