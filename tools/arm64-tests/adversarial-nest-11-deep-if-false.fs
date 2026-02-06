\ Adversarial test: 6 levels of nested IF with mixed conditions
\ Tests that false branches work at each level
\ expect: 32

: main
  0 if  \ false
    99
  else
    1 if  \ true
      0 if  \ false
        99
      else
        1 if  \ true
          0 if  \ false
            99
          else
            1 if  \ true
              32   \ this path
            else 99 then
          then
        else 99 then
      then
    else 99 then
  then
;
