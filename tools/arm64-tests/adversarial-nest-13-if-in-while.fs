\ Adversarial test: IF inside BEGIN-WHILE-REPEAT
\ Tests branching within iteration (simplified)
\ expect: 15

: main
  0           \ sum
  10          \ counter
  begin
    dup 0 >
  while
    dup 5 > if   \ if counter > 5
      swap 2 + swap   \ add 2 to sum
    else
      swap 1+ swap    \ add 1 to sum
    then
    1-        \ decrement counter
  repeat
  drop
;
\ counter 10..6: adds 2 each, 5 times = 10
\ counter 5..1: adds 1 each, 5 times = 5
\ Total: 10 + 5 = 15
