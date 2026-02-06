\ expect: 1
\ ADVERSARIAL: Two different opens in same word

: main
  s" /dev/null" 0 open-file drop drop
  s" /tmp" 0 open-file drop drop
  1
;
