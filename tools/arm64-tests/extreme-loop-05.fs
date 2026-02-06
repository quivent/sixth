\ expect: 0
\ Multiple UNLOOP calls - verify return stack handling
: multi-exit ( -- n )
  0
  10 0 do
    5 0 do
      i j + 3 > if
        unloop        \ clean inner loop
        unloop        \ clean outer loop
        exit
      then
    loop
  loop
  99                  \ should not reach
;
: main multi-exit ;
\ outer j=0: inner i=0,1,2,3 all have i+j<=3
\            inner i=4: 4+0=4>3, double unloop + exit
\ Returns 0 (accumulator was 0)
