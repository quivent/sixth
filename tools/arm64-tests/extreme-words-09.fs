\ expect: 42
\ Extreme Test 09: Word redefinition shadowing
\ Tests: symbol shadowing - each use should see definition current at compile time
\ BUG: Compiler uses FIRST definition, not most recent. Gets 45 instead of 42.
\ double should use compute=5 -> 10
\ triple should use compute=8 -> 24  (but compiler gives 15 = 5*3)
\ quad should use compute=2 -> 8     (but compiler gives 20 = 5*4)

: compute 5 ;
: double compute 2 * ;
: compute 8 ;
: triple compute 3 * ;
: compute 2 ;
: quad compute 4 * ;

: main double triple + quad + ;
