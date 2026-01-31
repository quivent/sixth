\ expect: 0 1 3 2 6 7 5 4
\ Gray code: G(n) = n XOR (n >> 1), print for 0-7
: gray ( n -- g ) dup 1 rshift xor ;
: main 8 0 do i gray . loop cr ;
