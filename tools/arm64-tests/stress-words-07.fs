\ stress-words-07.fs - Word redefines earlier word (test shadowing)
\ expect: 99
\ First 'val' returns 42, second 'val' shadows it and returns 99
: val 42 ;
: use-val val ;
: val 99 ;
: main val ;
