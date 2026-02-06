\ Adversarial test: Control structures spanning word calls
\ Tests that calls preserve control flow state
\ expect: 96

: double ( n -- n*2 ) 2 * ;
: add5 ( n -- n+5 ) 5 + ;

: inner-compute ( n -- result )
  dup 5 > if
    double
  else
    add5
  then
;

: outer-loop ( -- result )
  1           \ start value
  5 0 do
    inner-compute
  loop
;
\ i=0: 1 -> 1 <= 5 -> add5 -> 6
\ i=1: 6 -> 6 > 5 -> double -> 12
\ i=2: 12 -> 12 > 5 -> double -> 24
\ i=3: 24 -> 24 > 5 -> double -> 48
\ i=4: 48 -> 48 > 5 -> double -> 96

: main outer-loop ;
