\ expect: 1
\ Mutual recursion: is-even calls is-odd, is-odd calls is-even
: is-even dup 0= if drop 1 else 1- is-odd then ;
: is-odd dup 0= if drop 0 else 1- is-even then ;
: main 7 is-odd ;
