\ expect: 210
\ Test: Triple nested DO-LOOP with conditional LEAVE
\ BUG FOUND: "Unresolved forward reference: k" - inner loop index k not recognized

: main
  0                     \ sum
  7 1 do                \ i: 1-6
    7 1 do              \ j: 1-6
      7 1 do            \ k: 1-6
        i j k + + +     \ add i+j+k to sum - BUG: k not found
        i 3 = if
          j 3 = if
            k 3 = if
              leave     \ only exits innermost
            then
          then
        then
      loop
    loop
  loop
;
