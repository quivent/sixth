\ Adversarial test: 6 levels of nested IF
\ Tests deeply nested conditional branching
\ expect: 63

: main
  1 if
    1 if
      1 if
        1 if
          1 if
            1 if
              63   \ deepest level - all conditions true
            else 0 then
          else 0 then
        else 0 then
      else 0 then
    else 0 then
  else 0 then
;
