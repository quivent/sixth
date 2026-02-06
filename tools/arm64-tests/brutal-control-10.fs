\ expect: 0
\ Test: Ultimate stress - all control structures nested together

variable counter

: complex-control ( limit -- )
  0 counter !
  dup 0 do                          \ outer DO/LOOP
    i 2 mod 0= if                   \ IF inside loop
      begin
        counter @ i <               \ WHILE condition
      while
        counter @ 3 mod 0= if
          i 1+ 0 do                 \ nested DO/LOOP
            counter @ 1+ counter !
            counter @ 50 > if
              unloop unloop unloop exit    \ triple unloop + exit
            then
          loop
        else
          counter @ 1+ counter !
        then
      repeat
    else
      i 1 > if
        i 1 do                      \ different nested loop
          j i + counter @ + counter !
        loop
      then
    then
  loop
  drop
;

: verify ( -- result )
  20 complex-control
  counter @
;

: main
  verify
  verify
  = if 0 else 1 then                \ should be deterministic
;
