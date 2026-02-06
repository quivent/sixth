\ Adversarial control flow: 3-level nested if/else
\ Tests branch patching at multiple nesting depths
\ expect: 7
: main
  1 if
    1 if
      1 if 7 else 6 then
    else 5 then
  else 4 then ;
