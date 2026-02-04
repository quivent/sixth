\ expected: 184756
\ Generate all combinations C(20,10)

variable comb-count

: combine ( n k start -- ) recursive
  over 0= if 2drop drop 1 comb-count +! exit then
  over 2 pick - 2 pick + 1+ over > if 2drop drop exit then
  2 pick 1+ begin
    dup 3 pick <= while
    2 pick 3 pick 1- rot recurse
    1+
  repeat drop 2drop ;

: main
  0 comb-count !
  20 10 0 combine
  comb-count @ . cr ;
