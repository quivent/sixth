\ expect: 120
\ Edge case: K accessor in 3-deep nesting
\ Tests correct return stack offset for third loop level
: main
  0
  5 0 do                   \ k loop (outermost)
    4 0 do                 \ j loop
      3 0 do               \ i loop (innermost)
        k +                \ add outer k index
      loop
    loop
  loop
;
\ Inner body runs: 5 * 4 * 3 = 60 times
\ k values: 0,1,2,3,4 each for 12 iterations
\ sum = 12*(0+1+2+3+4) = 12*10 = 120
