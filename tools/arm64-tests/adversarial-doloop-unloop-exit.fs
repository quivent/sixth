\ Adversarial DO-LOOP test: unloop before early exit
\ Loop should clean return stack before exiting word early
\ The 99 should be returned, not anything from the loop
\ expect: 99
: find-three ( -- n )
  10 0 do
    i 3 = if unloop 99 exit then
  loop
  0 ;

: main find-three ;
