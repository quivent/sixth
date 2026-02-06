\ adversarial-loop-12-stack-between-i.fs - Stack operations between I accesses
\ Sum of i*2 for i=0,1,2,3,4: 0+2+4+6+8 = 20
\ expect: 20

: main
  0
  5 0 do
    i       \ push i
    i +     \ add i again (testing i doesn't corrupt stack)
    +       \ add to accumulator
  loop
;
