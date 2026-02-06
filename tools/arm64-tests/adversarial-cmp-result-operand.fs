\ Adversarial: Comparison result used as operand
\ Forth flags are 0 (false) or -1 (true), which can be used in arithmetic
\ expect: 1

: main
  \ -1 + -1 = -2 (two true comparisons added)
  1 1 =           \ true -> -1
  2 2 =           \ true -> -1
  +               \ -1 + -1 = -2
  -2 = 0= if 0 exit then

  \ Using flag to select: (flag AND a) OR ((NOT flag) AND b)
  \ If flag=-1: (-1 AND 10) = 10, (0 AND 20) = 0, result = 10
  5 5 =           \ true (-1)
  dup 10 and      \ -1 AND 10 = 10
  swap invert 20 and  \ 0 AND 20 = 0
  or
  10 = 0= if 0 exit then

  \ Arithmetic with false flag (0)
  1 2 =           \ false -> 0
  100 +           \ 0 + 100 = 100
  100 = 0= if 0 exit then

  \ Counting matches with flag accumulation
  0               \ accumulator
  1 1 = -         \ 1=1 is true(-1), 0-(-1)=1
  2 3 = -         \ 2=3 is false(0), 1-0=1
  4 4 = -         \ 4=4 is true(-1), 1-(-1)=2
  2 = 0= if 0 exit then

  \ Flag as array index: 0 or 1 (after negating -1 to 1)
  5 5 = negate    \ true(-1) negated = 1
  1 = 0= if 0 exit then

  1 2 = negate    \ false(0) negated = 0
  0= 0= if 0 exit then

  1 ;
