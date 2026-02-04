\ expected: 100000000
\ Hoist computation out of loop

: main
  3 7 +   \ loop-invariant computation = 10
  0 swap  \ sum, invariant
  100000000 0 do
    dup 10 = if swap 1+ swap then
  loop drop . cr ;
