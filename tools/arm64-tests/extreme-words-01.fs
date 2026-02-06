\ expect: 42
\ Extreme Test 01: 25-deep call chain
\ Tests: return stack depth, call/ret instruction generation

: w25 42 ;
: w24 w25 ;
: w23 w24 ;
: w22 w23 ;
: w21 w22 ;
: w20 w21 ;
: w19 w20 ;
: w18 w19 ;
: w17 w18 ;
: w16 w17 ;
: w15 w16 ;
: w14 w15 ;
: w13 w14 ;
: w12 w13 ;
: w11 w12 ;
: w10 w11 ;
: w9 w10 ;
: w8 w9 ;
: w7 w8 ;
: w6 w7 ;
: w5 w6 ;
: w4 w5 ;
: w3 w4 ;
: w2 w3 ;
: w1 w2 ;

: main w1 ;
