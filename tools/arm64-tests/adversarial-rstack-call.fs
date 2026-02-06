\ Adversarial test: return stack across word calls
\ Helper word uses its own return stack, shouldn't corrupt caller's
\ 33 saved to return stack, 10 doubled by helper = 20, then add 33 = 53
\ expect: 53
: helper ( n -- n*2 ) dup >r r> + ;
: main 33 >r 10 helper r> + ;
