\ Adversarial Stack Test 02: Stack operations near empty
\ expect: 0
\ Test operations that bring stack to minimal depth

: test-single
  42 drop 0 ;

: test-push-pop
  1 2 3 drop drop drop 0 ;

: test-swap-drop
  1 2 swap drop drop 0 ;

: test-over-drops
  1 2 over drop drop drop 0 ;

: main
  test-single
  test-push-pop +
  test-swap-drop +
  test-over-drops + ;
