\ Adversarial control flow: empty then branch (no-op if true)
\ Tests that empty conditional branches compile correctly
\ expect: 50
: main 50 1 if then ;
