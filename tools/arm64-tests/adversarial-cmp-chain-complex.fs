\ Adversarial: Complex comparison chains
\ Test sequences where multiple comparisons combine
\ expect: 1

: main
  \ Chain of comparisons: a < b AND b < c
  3 5 <           \ 3 < 5 -> true
  5 7 <           \ 5 < 7 -> true
  and
  -1 = 0= if 0 exit then

  \ Chain that should fail: a < b AND b > c
  3 5 <           \ 3 < 5 -> true
  5 7 >           \ 5 > 7 -> false
  and
  0= 0= if 0 exit then

  \ Multiple ANDed comparisons
  1 2 <           \ true
  2 3 <           \ true
  and
  3 4 <           \ true
  and
  -1 = 0= if 0 exit then

  \ Multiple ORed comparisons
  1 2 >           \ false
  2 1 >           \ true
  or
  -1 = 0= if 0 exit then

  \ Nested: (a < b) AND ((c < d) OR (e < f))
  1 2 <           \ true
  3 4 <           \ true
  5 6 >           \ false
  or              \ true OR false = true
  and             \ true AND true = true
  -1 = 0= if 0 exit then

  \ All false chain
  1 1 <           \ false
  2 2 <           \ false
  or
  0= 0= if 0 exit then

  1 ;
