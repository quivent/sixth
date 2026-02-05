\ Phase 11 test: leave (sets index=limit to exit on next iteration)
\ expect: 10
: main 10 0 do i 3 = if leave then i loop ;
