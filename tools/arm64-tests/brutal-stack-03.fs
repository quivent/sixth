\ expect: 0
\ Test: OVER must not consume the value it copies
\ Stack: a b -- a b a
\ After OVER, all three values must be accessible

: main
  100 200 over      ( Stack: 100 200 100 )
  ( Verify TOS=100: push 100, subtract should give 0 )
  100 -             ( Stack: 100 200 0 )
  ( Verify second item=200: swap, push 200, subtract )
  swap              ( Stack: 100 0 200 )
  200 -             ( Stack: 100 0 0 )
  or                ( Stack: 100 0 )
  ( Verify bottom=100 )
  swap              ( Stack: 0 100 )
  100 -             ( Stack: 0 0 )
  or                ( 0 or 0 = 0 )
;
