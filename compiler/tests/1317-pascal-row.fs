\ expect: 1 6 15 20 15 6 1
variable coeff
variable nn
: pascal-row ( n -- )
  dup nn !
  1 coeff !  coeff @ .
  1+ 1 do
    coeff @  nn @ i - 1+  *  i /  coeff !
    coeff @ .
  loop cr ;
: main 6 pascal-row ;
