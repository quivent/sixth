\ adversarial-loop-09-alternating-step.fs - +LOOP with alternating signs
\ Compute step based on toggle, use single +LOOP
\ i=0: toggle=0, step=3 -> i=3, toggle=1
\ i=3: toggle=1, step=1 -> i=4, toggle=0
\ i=4: toggle=0, step=3 -> i=7, toggle=1
\ i=7: toggle=1, step=1 -> i=8, toggle=0
\ i=8: toggle=0, step=3 -> i=11 >= 10, exit
\ 5 iterations
\ expect: 5

variable toggle

: main
  0 toggle !
  0   \ count
  10 0 do
    1+
    toggle @ if
      0 toggle !
      1
    else
      1 toggle !
      3
    then
    +loop
;
