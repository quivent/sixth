\ expect: 0
\ Test: Everything at once - the ultimate stress test
\ Variables, loops, return stack, control flow, strings

variable base1
variable acc
variable ptr
variable flag

variable d0  variable d1  variable d2  variable d3
variable d4  variable d5  variable d6  variable d7
variable d8  variable d9  variable d10 variable d11
variable d12 variable d13 variable d14 variable d15
variable d16 variable d17 variable d18 variable d19

: set-data ( val n -- )
  dup 0 = if drop d0 ! exit then
  dup 1 = if drop d1 ! exit then
  dup 2 = if drop d2 ! exit then
  dup 3 = if drop d3 ! exit then
  dup 4 = if drop d4 ! exit then
  dup 5 = if drop d5 ! exit then
  dup 6 = if drop d6 ! exit then
  dup 7 = if drop d7 ! exit then
  dup 8 = if drop d8 ! exit then
  dup 9 = if drop d9 ! exit then
  dup 10 = if drop d10 ! exit then
  dup 11 = if drop d11 ! exit then
  dup 12 = if drop d12 ! exit then
  dup 13 = if drop d13 ! exit then
  dup 14 = if drop d14 ! exit then
  dup 15 = if drop d15 ! exit then
  dup 16 = if drop d16 ! exit then
  dup 17 = if drop d17 ! exit then
  dup 18 = if drop d18 ! exit then
  dup 19 = if drop d19 ! exit then
  2drop ;

: get-data ( n -- val )
  dup 0 = if drop d0 @ exit then
  dup 1 = if drop d1 @ exit then
  dup 2 = if drop d2 @ exit then
  dup 3 = if drop d3 @ exit then
  dup 4 = if drop d4 @ exit then
  dup 5 = if drop d5 @ exit then
  dup 6 = if drop d6 @ exit then
  dup 7 = if drop d7 @ exit then
  dup 8 = if drop d8 @ exit then
  dup 9 = if drop d9 @ exit then
  drop 0 ;

: store-seq ( n -- )
  dup ptr !
  0 do
    base1 @ i + i set-data
  loop ;

: sum-data ( n -- sum )
  0 swap
  0 do
    i get-data +
  loop ;

: cond-proc ( n -- n' )
  dup 200 > if
    2 /
  else
    dup 100 > if
      10 -
    else
      1+
    then
  then ;

: rstack-work ( a b -- c )
  >r >r
  r@ r> +
  r> * ;

: mega ( -- result )
  100 base1 !
  0 acc !
  1 flag !
  10 store-seq
  10 sum-data acc !
  acc @ cond-proc acc !
  s" test" drop c@ acc @ + acc !
  acc @ 5 rstack-work
  flag @ + ;

: main
  mega
  \ store-seq(10): data[i] = 100+i for i=0..9
  \ sum-data(10): sum of 100..109 = 10*100 + (0+1+...+9) = 1000+45 = 1045
  \ cond-proc(1045): 1045 > 200, so 1045/2 = 522
  \ s" test" -> 't' = 116, acc = 522 + 116 = 638
  \ rstack-work(638, 5): ((5 + 5) * 638) = 10 * 638 = 6380
  \ + flag(1) = 6381
  6381 = if 0 else 1 then ;
