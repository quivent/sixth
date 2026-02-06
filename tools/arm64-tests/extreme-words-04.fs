\ expect: 50
\ Extreme Test 04: 50 small words all called from main
\ Tests: symbol table size, many call sites in one word

: a1 1 ; : a2 1 ; : a3 1 ; : a4 1 ; : a5 1 ;
: a6 1 ; : a7 1 ; : a8 1 ; : a9 1 ; : a10 1 ;
: b1 1 ; : b2 1 ; : b3 1 ; : b4 1 ; : b5 1 ;
: b6 1 ; : b7 1 ; : b8 1 ; : b9 1 ; : b10 1 ;
: c1 1 ; : c2 1 ; : c3 1 ; : c4 1 ; : c5 1 ;
: c6 1 ; : c7 1 ; : c8 1 ; : c9 1 ; : c10 1 ;
: d1 1 ; : d2 1 ; : d3 1 ; : d4 1 ; : d5 1 ;
: d6 1 ; : d7 1 ; : d8 1 ; : d9 1 ; : d10 1 ;
: e1 1 ; : e2 1 ; : e3 1 ; : e4 1 ; : e5 1 ;
: e6 1 ; : e7 1 ; : e8 1 ; : e9 1 ; : e10 1 ;

: main
  a1 a2 + a3 + a4 + a5 + a6 + a7 + a8 + a9 + a10 +
  b1 + b2 + b3 + b4 + b5 + b6 + b7 + b8 + b9 + b10 +
  c1 + c2 + c3 + c4 + c5 + c6 + c7 + c8 + c9 + c10 +
  d1 + d2 + d3 + d4 + d5 + d6 + d7 + d8 + d9 + d10 +
  e1 + e2 + e3 + e4 + e5 + e6 + e7 + e8 + e9 + e10 + ;
