\ expect: 1
\ ADVERSARIAL: Open /dev/null multiple times
\ Stress test that verifies paths are copied correctly repeatedly

: open-devnull ( -- flag )
  s" /dev/null" 0 open-file drop 0>= ;

: main
  open-devnull 0= if 0 exit then
  open-devnull 0= if 0 exit then
  open-devnull 0= if 0 exit then
  open-devnull 0= if 0 exit then
  open-devnull 0= if 0 exit then
  open-devnull 0= if 0 exit then
  open-devnull 0= if 0 exit then
  open-devnull 0= if 0 exit then
  open-devnull 0= if 0 exit then
  open-devnull 0= if 0 exit then
  1
;
