\ expect: 18
\ Deep nesting with I access at each level
: main
  0
  3 0 do              \ outer
    3 0 do            \ middle
      2 0 do          \ inner
        j +           \ middle index
      loop
    loop
  loop
;
