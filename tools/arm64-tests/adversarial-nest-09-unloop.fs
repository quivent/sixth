\ Adversarial test: Nested loops with UNLOOP
\ Tests proper cleanup of loop index on early exit
\ expect: 7

: search ( -- result )
  10 0 do
    i 7 = if
      i unloop exit
    then
  loop
  99
;

: main search ;
\ Finds 7 in the loop, returns it
