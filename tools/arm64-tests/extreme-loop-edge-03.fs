\ expect: 48
\ Edge case: 4-level deep nesting using I, J, K accessors
\ Tests return stack layout with 4 nested loops (8 cells on rstack)
: main
  0
  2 0 do                  \ level 4 (outermost) - k would be 4*8=32 offset
    3 0 do                \ level 3 - j at 2*8=16 offset
      4 0 do              \ level 2 - i at 0*8 offset
        2 0 do            \ level 1 (innermost) - i shadows outer i
          1+              \ count iterations
        loop
      loop
    loop
  loop
;
\ 2 * 3 * 4 * 2 = 48 iterations total
