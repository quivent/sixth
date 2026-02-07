\ expect: 0
\ Pick with varying index in a loop - brutal runtime index test
: main
  1 2 3 4 5
  \ Stack: 1 2 3 4 5 (top is 5)
  \ 0 pick = 5, 1 pick = 4, 2 pick = 3, 3 pick = 2, 4 pick = 1
  \ So i pick should equal (5 - i)

  0   \ accumulator for errors
  5 0 do
    \ Stack during loop: 1 2 3 4 5 acc
    \ i=0: 1 pick should be 5 (since acc is at top, 1 pick is 5)
    \ i=1: 2 pick should be 4
    \ i=2: 3 pick should be 3
    \ i=3: 4 pick should be 2
    \ i=4: 5 pick should be 1
    \ General: (i 1 +) pick should be (5 - i)
    i 1 + pick   \ get the value
    5 i - -      \ subtract expected value
    abs +        \ add absolute difference to accumulator
  loop
  \ Stack: 1 2 3 4 5 acc
  >r drop drop drop drop drop r>
;
