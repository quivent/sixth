\ expect: 0
\ Adversarial: chained min/max operations
\ Tests composition: min(max(...)), max(min(...))
\ Returns 0 if all tests pass
: main
  \ min of two maxes: max(3,7)=7, max(5,9)=9, min(7,9)=7
  3 7 max 5 9 max min 7 - abs

  \ max of two mins: min(3,7)=3, min(5,9)=5, max(3,5)=5
  3 7 min 5 9 min max 5 - abs +

  \ nested with negatives: max(-5,-10)=-5, max(0,-3)=0, min(-5,0)=-5
  -5 -10 max 0 -3 max min -5 - abs +

  \ nested: min(100,50)=50, min(25,75)=25, max(50,25)=50
  100 50 min 25 75 min max 50 - abs +
;
