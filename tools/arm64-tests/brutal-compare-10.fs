\ expect: 0
\ Test: Comparisons in control flow (IF/ELSE/THEN, loops)

variable counter

: main
  \ Simple IF with comparison
  5 3 > if else 1 exit then                 \ 5 > 3, so skip else

  \ IF with 0=
  0 0= if else 2 exit then                  \ 0 is zero, take if branch

  \ IF with 0<>
  42 0<> if else 3 exit then                \ 42 is nonzero, take if branch

  \ Counting loop with comparison as termination
  0 counter !
  begin
    counter @ 1+ counter !
    counter @ 10 >=
  until
  counter @ 10 <> if 4 exit then            \ Should have counted to 10

  \ WHILE loop with signed comparison
  0 counter !
  begin
    counter @ 5 <
  while
    counter @ 1+ counter !
  repeat
  counter @ 5 <> if 5 exit then             \ Should be exactly 5

  \ Nested IF with unsigned comparison
  -1 0 u>                                   \ MAX-UINT > 0 (unsigned)
  if
    1 0 u<                                  \ 1 U< 0 is false
    if 6 exit then
  else 7 exit then

  0
;
