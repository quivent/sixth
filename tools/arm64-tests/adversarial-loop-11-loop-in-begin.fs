\ adversarial-loop-11-loop-in-begin.fs - DO-LOOP inside BEGIN..UNTIL
\ Three rounds: sum 0..0=0, sum 0..1=1, sum 0..2=3
\ Total = 0+1+3 = 4
\ expect: 4

variable counter

: main
  0 counter !
  0   \ total accumulator
  begin
    counter @ 1+ counter !
    0
    counter @ 0 do
      i +
    loop
    +   \ add this round's sum to total
    counter @ 3 >=
  until
;
