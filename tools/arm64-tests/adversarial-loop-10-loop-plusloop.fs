\ adversarial-loop-10-loop-plusloop.fs - Combination of LOOP and +LOOP
\ Regular loop sum 1+2+3+4+5 = 15
\ +LOOP by 2 sum 0+2+4 = 6
\ Total = 21
\ expect: 21

: sum-by-1 ( -- n )
  0
  6 1 do i + loop  \ 1+2+3+4+5 = 15
;

: sum-by-2 ( -- n )
  0
  6 0 do i + 2 +loop  \ 0+2+4 = 6
;

: main
  sum-by-1 sum-by-2 +
;
