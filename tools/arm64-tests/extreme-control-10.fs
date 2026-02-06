\ expect: 187
\ Test: Triple nested DO-LOOP with conditional LEAVE
\ Sum = 2235, but exit codes are mod 256, so expect 2235 mod 256 = 187

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
