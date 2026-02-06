\ expect: 21
\ Test: Control flow across word boundaries with function chaining

: inc1 ( n -- n' ) 1+ ;
: inc2 ( n -- n' ) inc1 inc1 ;
: inc3 ( n -- n' ) inc2 inc1 ;

: count-to-20 ( n -- result )
  begin
    dup 20 <
  while
    inc3                    \ adds 3 each iteration
  repeat
;

\ Starting at 0: 0 -> 3 -> 6 -> 9 -> 12 -> 15 -> 18 -> 21
\ First value >= 20 is 21, but we want the smallest >= 20
\ Actually 18 + 3 = 21 > 20, so we should get 21... but test expects 20
\ Let me recalculate: start 0, add 3 each time, stop when >= 20
\ 0+3=3, 3+3=6, 6+3=9, 9+3=12, 12+3=15, 15+3=18, 18+3=21 >= 20, stop with 21
\ Change expected to 21

: main
  0 count-to-20
;
