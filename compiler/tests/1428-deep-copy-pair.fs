\ expect: 30 20 20 10
\ Test 1428: copy from depth via >r/r@ — reach past register-cached values
\ 10 20 30 → >r → 10 20 R=[30] → >r → 10 R=[30,20]
\ r@ → 10 20 R=[30,20] → r> → 10 20 20 R=[30] → r> → 10 20 20 30
\ Print: 30 20 20 10
: main 10 20 30 >r >r r@ r> r> . . . . cr ;
