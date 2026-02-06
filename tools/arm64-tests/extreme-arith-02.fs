\ expect: 100
\ MOD by zero - ARM64 quirk
\ SDIV gives 0, so remainder = n - (0 * 0) = n
\ This is "wrong" mathematically but matches ARM64 behavior
: main 100 0 mod ;
