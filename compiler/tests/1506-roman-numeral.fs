\ expect: 1994
\ Convert 1994 to roman numeral values and sum back
\ 1994 = M(1000) + CM(900) + XC(90) + IV(4)
variable val
variable total
: try-sub ( amount -- )
  val @ over >= if
    dup negate val +!
    total +!
  else drop then ;
: try-count ( amount -- )
  begin dup val @ <= while
    dup negate val +!
    dup total +!
  repeat drop ;
: main
  1994 val !  0 total !
  1000 try-count
  900 try-sub
  500 try-count
  400 try-sub
  100 try-count
  90 try-sub
  50 try-count
  40 try-sub
  10 try-count
  9 try-sub
  5 try-count
  4 try-sub
  1 try-count
  total @ . cr ;
