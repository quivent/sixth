\ Adversarial control flow: begin/until exits on first iteration
\ Tests immediate exit condition (no looping)
\ expect: 77
: main 77 begin 1 until ;
