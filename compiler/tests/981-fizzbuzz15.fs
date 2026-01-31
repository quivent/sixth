\ Test 981: FizzBuzz 1-15 using emit (F=70, B=66, space=32)
\ For each n: divisible by 15->FB, by 3->F, by 5->B, else print number
: fb dup 15 mod 0= if 70 emit 66 emit drop else dup 3 mod 0= if 70 emit drop else dup 5 mod 0= if 66 emit drop else . exit then then then 32 emit ;
: main 16 1 do i fb loop cr ;
