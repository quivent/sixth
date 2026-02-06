\ Adversarial test: r@ peeks without consuming
\ r@ twice should give same value
\ expect: 84
: main 42 >r r@ r@ + r> drop ;
