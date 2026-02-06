\ Adversarial test: return stack with negative value
\ -42 pushed and popped, verify it's still -42
\ expect: 1
: main -42 >r r> -42 = if 1 else 0 then ;
