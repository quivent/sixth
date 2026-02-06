\ Adversarial control flow: stack manipulation across branches
\ Tests that stack is consistent regardless of branch taken
\ expect: 30
: branch-sum ( a b c flag -- result )
  if
    + +      \ a+b+c if true
  else
    drop + + \ a+b if false, drop c
  then ;

: main 10 10 10 1 branch-sum ;
