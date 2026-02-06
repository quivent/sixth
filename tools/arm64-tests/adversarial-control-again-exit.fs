\ Adversarial control flow: begin/again with conditional exit via if
\ Tests unconditional loop with conditional escape
\ expect: 5
: count-to-5 ( n -- n )
  begin
    1 +
    dup 5 = if exit then
  again ;

: main 0 count-to-5 ;
