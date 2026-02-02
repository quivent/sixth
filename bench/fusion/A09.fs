\ expect: -4 7
\ Pattern A09: swap invert
\ swap then bitwise NOT — invert(3)=-4 — wrong register gives 3 -8
: main 3 7 swap invert . . cr ;
