\ Adversarial test: return stack with zero value
\ Ensure zero is not mishandled as empty or false
\ expect: 0
: main 0 >r r> ;
