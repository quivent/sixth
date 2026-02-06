\ expect: 1
\ LSHIFT by 64 bits - ARM64 masks to 6 bits
\ 64 AND 63 = 0, so effectively no shift
\ Potential bug: code might not handle this edge case
: main 1 64 lshift ;
