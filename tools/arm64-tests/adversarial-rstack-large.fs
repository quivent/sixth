\ Adversarial test: return stack with large values
\ Use values near max signed 64-bit: 9223372036854775807
\ Just test that we can push and pop a big number without corruption
\ expect: 1
: main 9223372036854775800 >r r> 9223372036854775800 = if 1 else 0 then ;
