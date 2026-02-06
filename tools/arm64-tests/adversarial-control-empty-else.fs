\ Adversarial control flow: empty else branch
\ Tests that empty else branch compiles correctly
\ expect: 60
: main 60 0 if drop 99 else then ;
