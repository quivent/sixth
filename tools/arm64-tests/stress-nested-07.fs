\ Stress test: Mixed ?DO and DO nesting
\ expect: 6
\ Outer ?DO: 3 iterations (limit > start, so runs)
\ Inner DO: 2 iterations each
\ 3 * 2 = 6
\ Note: ?DO checks if limit = start and skips if so
: main
  0                     \ accumulator
  3 0 ?do               \ ?DO: runs because 3 > 0
    2 0 do              \ regular DO: 2 iterations
      1 +               \ count
    loop
  loop ;
