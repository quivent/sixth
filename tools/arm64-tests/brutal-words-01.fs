\ expect: 0
\ Test: Forward reference - call word before definition
\ The word 'later' is called before it's defined

: early ( -- n ) later 1 + ;
: later ( -- n ) 42 ;
: main early 43 - ;
