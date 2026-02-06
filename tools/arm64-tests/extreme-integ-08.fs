\ expect: 0
\ Test: begin-while-repeat with variables and return stack
\ Loop termination with side effects everywhere

variable count
variable sum
variable limit

: accum-loop ( -- )
  begin
    count @ limit @ <
  while
    count @ >r
    r@ sum +!
    r> 1+ count !
  repeat ;

: outer-loop ( -- )
  3 0 do
    0 count !
    i 3 + limit !
    accum-loop
  loop ;

: main
  0 sum !
  outer-loop
  \ i=0: limit=3, sum += 0+1+2 = 3
  \ i=1: limit=4, sum += 0+1+2+3 = 6, total=9
  \ i=2: limit=5, sum += 0+1+2+3+4 = 10, total=19
  sum @ 19 = if 0 else 1 then ;
