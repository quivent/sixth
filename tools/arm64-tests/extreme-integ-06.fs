\ expect: 0
\ Test: Memory block ops with control flow interleaving
\ Variables as indexed array simulation

variable s0  variable s1  variable s2  variable s3
variable s4  variable s5  variable s6  variable s7
variable s8  variable s9  variable s10 variable s11
variable s12 variable s13 variable s14 variable s15
variable s16 variable s17 variable s18 variable s19

variable d0  variable d1  variable d2  variable d3
variable d4  variable d5  variable d6  variable d7
variable d8  variable d9  variable d10 variable d11
variable d12 variable d13 variable d14 variable d15
variable d16 variable d17 variable d18 variable d19

: init-src ( -- )
  65 s0 !   66 s1 !   67 s2 !   68 s3 !
  69 s4 !   70 s5 !   71 s6 !   72 s7 !
  73 s8 !   74 s9 !   75 s10 !  76 s11 !
  77 s12 !  78 s13 !  79 s14 !  80 s15 !
  81 s16 !  82 s17 !  83 s18 !  84 s19 ! ;

: get-s ( n -- val )
  dup 0 = if drop s0 @ exit then
  dup 1 = if drop s1 @ exit then
  dup 2 = if drop s2 @ exit then
  dup 3 = if drop s3 @ exit then
  dup 4 = if drop s4 @ exit then
  dup 5 = if drop s5 @ exit then
  dup 6 = if drop s6 @ exit then
  dup 7 = if drop s7 @ exit then
  dup 8 = if drop s8 @ exit then
  dup 9 = if drop s9 @ exit then
  drop 0 ;

: set-d ( val n -- )
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
  2drop ;

: copy-cond ( -- )
  10 0 do
    i 2 mod 0= if
      i get-s i set-d
    else
      0 i set-d
    then
  loop ;

: calc-sum ( -- n )
  d0 @ d2 @ + d4 @ + d6 @ + d8 @ + ;

: main
  init-src
  copy-cond
  calc-sum
  \ Even positions 0,2,4,6,8 -> chars 65,67,69,71,73
  \ Sum = 65+67+69+71+73 = 345
  345 = if 0 else 1 then ;
