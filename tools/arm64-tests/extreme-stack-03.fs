\ expect: 1
\ Test: Deep nested IF with stack ops at each level
: main
  1 1 1 1 1 1 1 1 1 1
  if
    dup if
      dup if
        dup if
          dup if
            dup if
              dup if
                dup if
                  dup if
                    dup if
                      drop drop drop drop drop
                      drop drop drop drop
                      1
                    else 0 then
                  else 0 then
                else 0 then
              else 0 then
            else 0 then
          else 0 then
        else 0 then
      else 0 then
    else 0 then
  else 0 then
;
