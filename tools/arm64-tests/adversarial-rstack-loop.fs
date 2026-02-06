\ Adversarial test: return stack usage in begin/until loop
\ Use r@ to count down from 5 to 0, sum = 5+4+3+2+1 = 15
\ expect: 15
: main
  0 5 >r
  begin
    r@ +
    r> 1 - dup >r
    0=
  until
  r> drop ;
