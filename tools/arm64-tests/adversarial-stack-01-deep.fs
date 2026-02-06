\ Adversarial Stack Test 01: Deep stack (50+ items)
\ expect: 0
\ Test pushing 60 items and verifying stack integrity

: push-60 ( -- n1...n60 )
  1 2 3 4 5 6 7 8 9 10
  11 12 13 14 15 16 17 18 19 20
  21 22 23 24 25 26 27 28 29 30
  31 32 33 34 35 36 37 38 39 40
  41 42 43 44 45 46 47 48 49 50
  51 52 53 54 55 56 57 58 59 60 ;

: verify-sum ( n1...n60 -- sum )
  \ Sum of 1..60 = 1830
  + + + + + + + + + +   \ sum 10
  + + + + + + + + + +   \ sum 10
  + + + + + + + + + +   \ sum 10
  + + + + + + + + + +   \ sum 10
  + + + + + + + + + +   \ sum 10
  + + + + + + + + + ;   \ sum 9 (total 59 adds)

: main
  push-60 verify-sum
  1830 = if 0 else 1 then ;
