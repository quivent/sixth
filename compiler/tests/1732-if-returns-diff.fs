\ expect: 10 20 10
: choose ( flag -- n )
  if 10 else 20 then
;
: main
  1 choose .
  0 choose .
  1 choose .
  cr
;
