\ expect: 10
\ N choose K: C(5,2) = 10
\ C(n,k) = n! / (k! * (n-k)!)
\ Iterative: C(n,k) = product(i=0..k-1) of (n-i)/(i+1)
: nchoosek ( n k -- result )
  1 swap                \ ( n 1 k )
  0 do                  \ ( n result )
    over i - *          \ result *= (n - i)
    i 1+ /              \ result /= (i + 1)
  loop nip ;
: main 5 2 nchoosek . cr ;
