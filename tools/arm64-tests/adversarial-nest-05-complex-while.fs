\ Adversarial test: BEGIN-UNTIL with nested IF-ELSE
\ Tests conditional branching inside loop
\ expect: 15

: main
  0           \ accumulator
  6           \ counter
  begin
    dup 4 > if
      \ path A: counter 6,5 -> add counter
      dup rot + swap  \ acc = acc + counter
    else
      dup 2 > if
        \ path B: counter 4,3 -> add 1
        swap 1+ swap
      else
        \ path C: counter 2,1 -> add 1
        swap 1+ swap
      then
    then
    1-              \ decrement counter
    dup 0=          \ until counter = 0
  until
  drop
;
\ counter=6: path A, acc += 6 -> 6
\ counter=5: path A, acc += 5 -> 11
\ counter=4: path B, acc += 1 -> 12
\ counter=3: path B, acc += 1 -> 13
\ counter=2: path C, acc += 1 -> 14
\ counter=1: path C, acc += 1 -> 15
