\ Adversarial DO-LOOP test: negative step with +loop
\ Standard Forth: loop exits when index crosses limit boundary
\ For positive step: exit when new_index >= limit
\ For negative step: exit when new_index < limit
\ Here: start=10, limit=0, step=-2
\ i=10: body runs, i becomes 8
\ i=8: body runs, i becomes 6
\ ...
\ i=2: body runs, i becomes 0
\ i=0: 0 < 0 is false... depends on signed comparison
\ This tests if the compiler handles negative step correctly
\ Current impl uses LT which works for positive steps only
\ i values executed: 10, 8, 6, 4, 2 (5 iterations), sum=30
\ But if impl is wrong, might get different result
\ expect: 30
: main 0 0 10 do i + -2 +loop ;
