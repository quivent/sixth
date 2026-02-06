\ expect: 9
\ Test: Nested DO/LOOP with I and J indices

: matrix-sum ( -- sum )
  0                         \ sum
  3 0 do                    \ j = 0,1,2 (outer loop)
    3 0 do                  \ i = 0,1,2 (inner loop)
      i j * +               \ add i*j to sum
    loop
  loop
;

\ j=0: i=0,1,2 -> 0*0 + 1*0 + 2*0 = 0
\ j=1: i=0,1,2 -> 0*1 + 1*1 + 2*1 = 3
\ j=2: i=0,1,2 -> 0*2 + 1*2 + 2*2 = 6
\ total = 9

: main
  matrix-sum
;
