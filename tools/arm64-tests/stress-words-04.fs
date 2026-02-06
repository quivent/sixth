\ stress-words-04.fs - Deeply nested word calls with return stack ops
\ expect: 31
\ layer1(n) = 2n+1, layer2(n) = 2n+11, ..., layer5(n) = 2n+23
\ main = layer5(4) = 2*4+23 = 31
: layer1 dup >r 1+ r> + ;
: layer2 >r 5 r> + layer1 ;
: layer3 >r 3 r> + layer2 ;
: layer4 >r 2 r> + layer3 ;
: layer5 >r 1 r> + layer4 ;
: main 4 layer5 ;
