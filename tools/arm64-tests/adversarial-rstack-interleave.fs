\ Adversarial test: interleaved return stack and data stack ops
\ Test: push data, >r, more data ops, r>, verify correct values
\ expect: 27
: main 10 5 + >r 3 4 * r> + ;
