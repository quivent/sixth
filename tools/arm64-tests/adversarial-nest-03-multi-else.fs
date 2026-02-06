\ Adversarial test: Multiple ELSE branches in sequence
\ Tests cascading if-else chains
\ expect: 42

: classify ( n -- code )
  dup 100 > if drop 10 else
  dup 50 > if drop 20 else
  dup 25 > if drop 30 else
  dup 10 > if drop 42 else
  dup 5 > if drop 50 else
  dup 0 > if drop 60 else
  drop 70
  then then then then then then
;

: main
  15 classify  \ 15 > 10, so returns 42
;
