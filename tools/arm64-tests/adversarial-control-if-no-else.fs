\ Adversarial control flow: if without else, value preserved through
\ Tests that stack value survives when condition is false
\ expect: 42
: main 42 0 if drop 99 then ;
