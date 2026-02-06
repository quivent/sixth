\ expect: 120
\ Test: Factorial via recursion with multiple IF branches
\ Deep call stack plus control flow

: fact ( n -- n! )
  dup 0= if
    drop 1
  else
    dup 1 = if
      drop 1
    else
      dup 1- fact *
    then
  then
;

: main
  5 fact
;
