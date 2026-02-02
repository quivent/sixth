\ expect: 3
\ Pattern A23: swap rshift
\ ADVERSARIAL: 24>>3=3 — without swap: 3>>24=0
: main 3 24 swap rshift . cr ;
