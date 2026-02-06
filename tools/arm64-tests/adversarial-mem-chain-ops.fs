\ expect: 54
\ ADVERSARIAL: Chained memory operations without intermediate drops
\ Tests complex sequences: store, increment, fetch, compute
\ Pattern: store 100, +! 10 five times, fetch and add offset
\ 310 mod 256 = 54

variable x
variable y

: main
  100 x !                 \ x = 100
  200 y !                 \ y = 200
  10 x +!                 \ x = 110
  10 x +!                 \ x = 120
  10 x +!                 \ x = 130
  -30 y +!                \ y = 170
  20 y +!                 \ y = 190
  x @ y @ -               \ 130 - 190 = -60
  x @ +                   \ -60 + 130 = 70
  y @ +                   \ 70 + 190 = 260
  50 +                    \ 260 + 50 = 310
  310 = if 54 else 0 then  \ return 54 if correct
;
