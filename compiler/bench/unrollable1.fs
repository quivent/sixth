\ expected: 1250000025000000
\ Simple unrollable loop - GCC unrolls fixed-count loops

: sum-to ( n -- sum )
  0 swap 1+ 0 do i + loop ;

: main 50000000 sum-to . cr ;
