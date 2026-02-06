\ expect: 50
\ ADVERSARIAL: Test +! with negative values
\ Verifies +! handles subtraction via negative increment
\ Initial 100, add -50, should result in 50

variable counter

: main
  100 counter !           \ start at 100
  -50 counter +!          \ add -50 (effectively subtract)
  counter @               \ should be 50
;
