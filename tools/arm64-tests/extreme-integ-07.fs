\ expect: 0
\ Test: Variables in complex expressions
\ Runtime computation stress without constants

variable magic
variable mult
variable div1
variable runtime
variable combo

: init-const ( -- )
  42 magic !
  7 mult !
  3 div1 ! ;

: const-expr ( n -- n' )
  magic @ +
  mult @ *
  div1 @ / ;

: var-expr ( -- n )
  runtime @
  magic @ *
  combo @ + ;

: mixed ( -- n )
  init-const
  10 runtime !
  5 combo !
  1 const-expr
  runtime !
  var-expr ;

: main
  mixed
  \ const-expr(1) = ((1+42)*7)/3 = (43*7)/3 = 301/3 = 100
  \ runtime = 100
  \ var-expr = 100*42 + 5 = 4205
  4205 = if 0 else 1 then ;
