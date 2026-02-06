\ Adversarial DO-LOOP test: nested loops with i and j
\ Outer: 0,1,2 (3 times), Inner: 0,1 (2 times each)
\ Sum all i*10+j combinations:
\ j=0: i=0: 0, i=1: 1  (inner sums 0+1=1)
\ j=1: i=0: 0, i=1: 1  (inner sums 0+1=1)
\ j=2: i=0: 0, i=1: 1  (inner sums 0+1=1)
\ Actually: outer j=0,1,2, inner i=0,1
\ sum = (0+1) + (0+1) + (0+1) = 3 for i
\ sum j*2 each: 0*2 + 1*2 + 2*2 = 0+2+4 = 6 for j contribution
\ Let's do: sum of (j*10 + i) for j in 0..2, i in 0..1
\ j=0: 0+1 = 1
\ j=1: 10+11 = 21
\ j=2: 20+21 = 41
\ total = 1+21+41 = 63
\ expect: 63
: main 0 3 0 do 2 0 do j 10 * i + + loop loop ;
