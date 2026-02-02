\ expect: 5 7
\ Pattern A07: swap negate
\ swap then negate — negate(-5)=5 — wrong register gives -5 -7
: main -5 7 swap negate . . cr ;
