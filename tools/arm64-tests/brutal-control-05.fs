\ expect: 20
\ Test: +LOOP with varying increments

: sum-evens ( limit -- sum )
  \ Sum even numbers from 0 up to (but not including) limit
  0 swap                    \ sum limit
  0 do                      \ start at 0
    i +                     \ add i to sum
    2                       \ step by 2
  +loop
;

\ 0 + 2 + 4 + 6 + 8 = 20

: main
  10 sum-evens
;
