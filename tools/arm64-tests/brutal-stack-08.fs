\ expect: 0
\ Test: Interleaved operations that stress register allocation
\ Complex sequence that exposes TOS caching bugs

: main
  99999999 11111111
  over          ( Stack: 99999999 11111111 99999999 )
  rot           ( Stack: 11111111 99999999 99999999 )
  swap          ( Stack: 11111111 99999999 99999999 - swapped same vals )
  drop          ( Stack: 11111111 99999999 )
  tuck          ( Stack: 99999999 11111111 99999999 )
  nip           ( Stack: 99999999 99999999 )
  -
;
