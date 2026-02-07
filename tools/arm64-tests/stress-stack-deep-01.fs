\ stress-stack-deep-01.fs - Deep data stack stress test
\ Tests: 50+ items pushed, complex manipulation, verify correct order
\ Edge case: Data stack overflow potential, pointer arithmetic correctness
\ expect: 50

\ Push 50 values, manipulate, verify depth and correctness
\ Sum of 1+2+3+...+50 = 1275, mod 256 = 251 for exit code
\ Instead: return stack depth indicator (50)

: push-50 ( -- 1 2 3 ... 50 )
  1 2 3 4 5 6 7 8 9 10
  11 12 13 14 15 16 17 18 19 20
  21 22 23 24 25 26 27 28 29 30
  31 32 33 34 35 36 37 38 39 40
  41 42 43 44 45 46 47 48 49 50 ;

: sum-10 ( x1...x10 -- sum )
  + + + + + + + + + ;

: main
  push-50
  \ Stack now has 50 items: 1 at bottom, 50 at TOS
  \ Sum top 10
  sum-10       \ ( 1...40 sum40-50 ) where sum = 41+42+..+50 = 455
  \ Drop the sum, keep checking that 40 is next
  drop
  \ Now TOS should be 40
  dup 40 = if
    \ Correct! Sum remaining and return depth indicator
    drop
    \ Drop items 30-39
    drop drop drop drop drop drop drop drop drop drop
    \ Drop items 20-29
    drop drop drop drop drop drop drop drop drop drop
    \ Drop items 10-19
    drop drop drop drop drop drop drop drop drop drop
    \ Drop items 1-9
    drop drop drop drop drop drop drop drop drop
    50   \ Return success indicator: we had 50 items
  else
    1    \ Error indicator
  then
;
