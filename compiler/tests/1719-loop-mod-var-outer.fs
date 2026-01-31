\ expect: 5 4 3 2 1
variable counter
: main
  5 counter !
  begin counter @ 0> while
    counter @ .
    counter @ 1- counter !
  repeat
  cr
;
