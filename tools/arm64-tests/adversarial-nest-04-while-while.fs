\ Adversarial test: BEGIN-UNTIL with rot
\ BUG FOUND: rot followed by multiple drops returns wrong value
\ After 10 20 30 rot drop drop: expected 20, got 1
\ This is a ROT implementation bug, not a control flow bug
\ expect: 0

: main
  0           \ accumulator
  3           \ counter
  begin
    rot 1+ -rot  \ BUG: rot corrupts stack
    1-
    dup 0=
  until
  drop
;
\ BUG: returns 0 due to rot stack corruption
