\ expected: 1000000000
\ Constant folding verification - all arithmetic should fold

: main
  0 1000000000 0 do
    3 4 + 2 * 5 - 1+    \ should fold to 10
    10 = if 1+ then
  loop . cr ;
