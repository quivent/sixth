\ expect: 21
\ Mutual recursion: count down from 6 alternating between A and B
\ A decrements and calls B; B decrements and calls A
\ Each decrement adds the current value
: count-a dup 0= if exit then dup 1- count-b + ;
: count-b dup 0= if exit then dup 1- count-a + ;
: main 6 count-a ;
