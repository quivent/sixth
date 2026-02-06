\ expect: 1
\ ADVERSARIAL: Rapid succession of short opens
\ Tests that path buffer is properly overwritten between calls
\ If previous path data leaks, we might get unexpected behavior

: main
  \ Open several short paths in rapid succession
  s" /" 0 open-file drop drop
  s" /t" 0 open-file drop drop
  s" /tm" 0 open-file drop drop
  s" /tmp" 0 open-file drop drop
  s" /" 0 open-file drop drop
  s" /d" 0 open-file drop drop
  s" /de" 0 open-file drop drop
  s" /dev" 0 open-file drop drop
  s" /dev/null" 0 open-file drop
  0>= if 1 else 0 then  \ final /dev/null should succeed
;
