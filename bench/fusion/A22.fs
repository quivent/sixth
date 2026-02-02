\ expect: 16
\ Pattern A22: swap lshift
\ ADVERSARIAL: 2<<3=16 — without swap: 3<<2=12
: main 3 2 swap lshift . cr ;
