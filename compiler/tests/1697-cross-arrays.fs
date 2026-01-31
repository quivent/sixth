\ expect: 12 20 42
\ Cross-reference: keys index into vals, multiply by position+1
create vals 40 allot
create keys 24 allot
: v! ( val i -- ) cells vals + ! ;
: v@ ( i -- val ) cells vals + @ ;
: k! ( val i -- ) cells keys + ! ;
: k@ ( i -- val ) cells keys + @ ;
: main
  10 0 v!  11 1 v!  12 2 v!  13 3 v!  14 4 v!
  2 0 k!  0 1 k!  4 2 k!
  3 0 do
    i k@ v@ i 1+ * .
  loop cr ;
