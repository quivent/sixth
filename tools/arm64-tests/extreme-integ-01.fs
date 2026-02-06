\ expect: 0
\ Test: Variables + loops + return stack torture
\ Hammer variables inside nested loops with >r/r> crossing loop boundaries

variable acc
variable outer
variable inner

: torture ( -- )
  0 acc !
  5 0 do
    i outer !
    outer @ >r
    3 0 do
      i inner !
      r@ inner @ * acc +!
    loop
    r> drop
  loop ;

: main
  torture
  \ Inner i shadows outer i. r@ gets current inner i value, not outer!
  \ Each outer iteration: (0*0 + 1*1 + 2*2) = 5
  \ 5 iterations = 25
  acc @ 25 = if 0 else 1 then ;
