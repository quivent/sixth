\ adversarial-loop-06-unloop-exit.fs - UNLOOP followed by EXIT
\ Find 5 in loop 0-9, return it
\ expect: 5

: find-it ( -- n )
  10 0 do
    i 5 = if
      i unloop exit
    then
  loop
  99  \ never reached
;

: main
  find-it
;
