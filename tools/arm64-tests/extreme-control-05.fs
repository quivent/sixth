\ expect: 42
\ Test: Control flow with empty IF bodies (not THEN)
\ Parser must handle if with empty body before else

: main
  1 if
  else
    99
  then
  0 if
    88
  else
  then
  drop
  0 if
  else
    42
  then
;
