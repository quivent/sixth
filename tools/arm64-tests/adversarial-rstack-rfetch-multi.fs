\ Adversarial test: r@ multiple times on same value
\ 7 * 5 = 35
\ expect: 35
: main 7 >r r@ r@ r@ r@ r@ + + + + r> drop ;
