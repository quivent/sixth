\ expect: 3
\ LEAVE from deeply nested - verify return stack cleanup
: main
  0
  100 0 do            \ outer - would run 100 times
    100 0 do          \ middle - would run 100 times
      100 0 do        \ inner - would run 100 times
        1+
        dup 3 = if
          leave       \ exits inner only
          leave       \ DEAD CODE - should this matter?
        then
      loop
      leave           \ exits middle
    loop
    leave             \ exits outer
  loop
;
\ First 3 inner iterations: count=3, then leave inner, leave middle, leave outer
