\ expect: 3
\ State machine: count words in "hi there bob" (3 words)
\ States: 0=space, 1=word. Transition space->word increments count
create input 16 allot
variable state
variable wcount
: process ( c -- )
  32 = if     \ space
    0 state !
  else
    state @ 0= if
      wcount @ 1+ wcount !
    then
    1 state !
  then ;
: main
  104 input c!  105 input 1+ c!   \ hi
  32 input 2+ c!                   \ space
  116 input 3 + c!  104 input 4 + c!  101 input 5 + c!
  114 input 6 + c!  101 input 7 + c!  \ there
  32 input 8 + c!                  \ space
  98 input 9 + c!  111 input 10 + c!  98 input 11 + c!  \ bob
  0 state !  0 wcount !
  12 0 do
    input i + c@ process
  loop
  wcount @ . cr ;
