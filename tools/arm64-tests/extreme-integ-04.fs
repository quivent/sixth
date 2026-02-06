\ expect: 0
\ Test: Multiple variables interacting through complex control
\ State machine with variable mutation at every step - no case/endcase

variable state
variable counter
variable result
variable temp

: trans1 ( -- )
  state @ 1+ state !
  counter @ 2 * counter !
  result @ counter @ + result ! ;

: trans2 ( -- )
  state @ 2 + state !
  counter @ 1+ counter !
  temp @ result @ + temp ! ;

: dispatch ( -- )
  state @ 3 mod
  dup 0 = if drop trans1 else
  dup 1 = if drop trans2 else
  drop trans1 trans2
  then then ;

: machine ( -- )
  0 state !
  1 counter !
  0 result !
  0 temp !
  10 0 do
    dispatch
  loop ;

: verify ( -- n )
  state @ temp @ + result @ + counter @ + ;

: main
  machine
  verify
  \ Complex state transitions - computed value
  dup 0> if drop 0 else drop 1 then ;
