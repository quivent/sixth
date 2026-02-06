\ expect: 77
\ Test: 12 levels of nested IF-THEN-ELSE
\ Each level tests a different bit pattern

: main
  1 if
    1 if
      1 if
        1 if
          1 if
            1 if
              1 if
                1 if
                  1 if
                    1 if
                      1 if
                        1 if
                          77
                        else
                          0
                        then
                      else
                        1
                      then
                    else
                      2
                    then
                  else
                    3
                  then
                else
                  4
                then
              else
                5
              then
            else
              6
            then
          else
            7
          then
        else
          8
        then
      else
        9
      then
    else
      10
    then
  else
    11
  then
;
