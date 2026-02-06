\ Adversarial Stack Test 11: Return stack operations
\ expect: 0
\ Test >r, r>, r@ interactions with data stack

: t-basic-r ( -- flag )
  42 >r r>
  42 = if 0 else 1 then ;

: t-r-fetch ( -- flag )
  100 >r r@ r@ r>
  \ Stack: 100 100 100
  100 = swap 100 = and swap 100 = and
  if 0 else 1 then ;

: t-r-pres ( -- flag )
  1 2 3
  >r >r
  \ Data: 1, Return: 3 2
  1 = if
    r> r>
    \ Data: 2 3
    3 = swap 2 = and
    if 0 else 1 then
  else
    r> r> drop drop 1
  then ;

: t-r-ops ( -- flag )
  10 20 >r
  5 +
  \ Data: 15, Return: 20
  r>
  \ Data: 15 20
  20 = swap 15 = and
  if 0 else 1 then ;

: t-r-nest ( -- flag )
  1 >r 2 >r 3 >r
  r> r> r>
  \ Popped in reverse: 3 2 1
  1 = swap 2 = and swap 3 = and
  if 0 else 1 then ;

: main
  t-basic-r
  t-r-fetch +
  t-r-pres +
  t-r-ops +
  t-r-nest + ;
