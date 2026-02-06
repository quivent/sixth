\ Adversarial control flow: stack manipulation inside loop
\ Tests that dup/drop survive loop iterations
\ Loop: add counter to accumulator 3 times
\ 0 + 3 + 2 + 1 = 6
\ expect: 6
: main
  0 3   \ accumulator counter
  begin dup 0> while
    swap over + swap  \ acc = acc + counter
    1 -
  repeat drop ;
