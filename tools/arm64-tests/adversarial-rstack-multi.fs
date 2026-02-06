\ Adversarial test: multiple values on return stack (LIFO order)
\ Push 10, 20, 30 - pop should give 30, 20, 10
\ Result: 30 + 20*2 + 10*3 = 30 + 40 + 30 = 100
\ expect: 100
: main 10 >r 20 >r 30 >r r> r> 2 * + r> 3 * + ;
