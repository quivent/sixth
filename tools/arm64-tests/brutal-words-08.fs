\ expect: 0
\ Test: Variables across word boundaries
\ Multiple words reading/writing same variable

variable counter

: reset-counter 0 counter ! ;
: inc-counter counter @ 1 + counter ! ;
: dec-counter counter @ 1 - counter ! ;
: add-to-counter ( n -- ) counter @ + counter ! ;
: get-counter ( -- n ) counter @ ;

: check ( -- n )
  reset-counter
  inc-counter inc-counter inc-counter
  get-counter 3 <> if 1 exit then
  dec-counter
  get-counter 2 <> if 2 exit then
  10 add-to-counter
  get-counter 12 <> if 3 exit then
  0 ;

: main check ;
