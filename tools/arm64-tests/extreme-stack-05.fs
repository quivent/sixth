\ expect: 6
\ Test: rot/-rot stress - tests 3-element rotation
: main
  1 2 3
  rot rot rot
  -rot -rot -rot
  rot -rot rot -rot rot -rot
  rot rot rot rot rot rot
  -rot -rot -rot -rot -rot -rot
  rot
  + +
;
