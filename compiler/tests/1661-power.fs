\ expect: 1024
\ Iterative power: 2^10
: power ( base exp -- result )
  1 swap             \ ( base 1 exp )
  0 do over * loop   \ multiply by base exp times
  nip ;
: main 2 10 power . cr ;
