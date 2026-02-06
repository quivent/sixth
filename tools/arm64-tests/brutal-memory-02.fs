\ expect: 0
\ Test: +! atomic increment across multiple operations
\ Verify cumulative effect and no value loss

variable counter
variable fail-code

: main
  0 fail-code !
  0 counter !

  \ Increment 100 times by 1
  100 0 do 1 counter +! loop
  counter @ 100 <> if 1 fail-code ! then

  \ Increment 50 times by -2 (decrement)
  fail-code @ 0= if
    50 0 do -2 counter +! loop
    counter @ 0<> if 2 fail-code ! then
  then

  \ Large increments
  fail-code @ 0= if
    1000000 counter +!
    -999999 counter +!
    counter @ 1 <> if 3 fail-code ! then
  then

  fail-code @ ;
