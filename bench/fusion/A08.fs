\ expect: 5 7
\ Pattern A08: swap abs
\ swap then abs — abs(-5)=5 — wrong register gives -5 7
: main -5 7 swap abs . . cr ;
