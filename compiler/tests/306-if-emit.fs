\ expect: *
\ Test: emit inside if branch → prints * (ASCII 42)
: main 1 if 42 emit else 63 emit then cr ;
