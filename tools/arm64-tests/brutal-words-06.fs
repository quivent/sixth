\ expect: 0
\ Test: Many small words - 20 tiny words each doing minimal work
\ Tests word table capacity and call overhead

: w1 1 ; : w2 2 ; : w3 3 ; : w4 4 ; : w5 5 ;
: w6 6 ; : w7 7 ; : w8 8 ; : w9 9 ; : w10 10 ;
: w11 11 ; : w12 12 ; : w13 13 ; : w14 14 ; : w15 15 ;
: w16 16 ; : w17 17 ; : w18 18 ; : w19 19 ; : w20 20 ;

: sum-all ( -- n )
  w1 w2 + w3 + w4 + w5 + w6 + w7 + w8 + w9 + w10 +
  w11 + w12 + w13 + w14 + w15 + w16 + w17 + w18 + w19 + w20 + ;

: main sum-all 210 - ;
