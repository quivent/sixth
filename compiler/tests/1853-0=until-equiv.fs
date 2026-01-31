\ expect: 3 2 1 3 2 1
\ 0=until compared to begin/0=/until equivalent: both should produce same result
\ Method 1: using 0=until
: count1 3 begin dup . 1- dup 0=until drop ;
\ Method 2: using begin/until with 0=
: count2 3 begin dup . 1- dup 0= until drop ;
: main count1 count2 cr ;
