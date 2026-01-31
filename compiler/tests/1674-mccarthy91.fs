\ expect: 91 91 91
\ McCarthy 91 function: M(n) = n-10 if n>100, else M(M(n+11))
: m91 ( n -- result )
  dup 100 > if 10 - exit then
  11 + m91 m91 ;
: main
  99 m91 .
  91 m91 .
  50 m91 . cr ;
