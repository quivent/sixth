: harmonic-sum ( -- sum ) 0 11 1 do 10000 i / + loop ;
: main harmonic-sum . cr ;
