\ Adversarial test: deep return stack (6 items)
\ Push 1,2,3,4,5,6 then pop all, verify LIFO order
\ Result: 6 + 5 + 4 + 3 + 2 + 1 = 21
\ expect: 21
: main
  1 >r 2 >r 3 >r 4 >r 5 >r 6 >r
  r> r> + r> + r> + r> + r> + ;
