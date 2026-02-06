\ Adversarial: Comparison in loops with boundary conditions
\ expect: 1

: count-to-zero ( n -- count )
  \ Count iterations from n down to 0 (inclusive)
  0 swap                  \ count n
  begin
    dup 0>=               \ while n >= 0
  while
    swap 1+ swap          \ count++
    1-                    \ n--
  repeat
  drop ;

: count-from-neg ( n -- count )
  \ Count iterations from negative n up to 0 (inclusive)
  0 swap                  \ count n
  begin
    dup 0<=               \ while n <= 0
  while
    swap 1+ swap          \ count++
    1+                    \ n++
  repeat
  drop ;

: find-1st-pos ( -- n )
  \ Start at -5, find first positive
  -5
  begin
    dup 0<=
  while
    1+
  repeat ;

: main
  \ Count from 3 to 0: iterations for 3,2,1,0 = 4
  3 count-to-zero
  4 = 0= if 0 exit then

  \ Count from 0 to 0: just 1 iteration (0 itself)
  0 count-to-zero
  1 = 0= if 0 exit then

  \ Count from -3 to 0: iterations for -3,-2,-1,0 = 4
  -3 count-from-neg
  4 = 0= if 0 exit then

  \ Find first positive starting from -5: should be 1
  find-1st-pos
  1 = 0= if 0 exit then

  1 ;
