\ expect: -2
\ Test 791: 10 dup 1- swap 1+ - => 10 9 swap(9 10) 1+(9 11) -(9-11=-2)... wait
\ 10 dup => 10 10, 1- => 10 9, swap => 9 10, 1+ => 9 11, - => 9-11 = -2
: main 10 dup 1- swap 1+ - . cr ;
