\ expect: 6
\ Loop variable used as loop limit for nested loop
: main
  0
  4 1 do            \ i = 1, 2, 3
    i 0 do          \ inner limit = outer i
      1+
    loop
  loop
;
\ i=1: inner 0..0 = 1 iter
\ i=2: inner 0..1 = 2 iters
\ i=3: inner 0..2 = 3 iters
\ total = 6
