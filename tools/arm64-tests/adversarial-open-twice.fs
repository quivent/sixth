\ expect: 1
\ Test opening the same file twice without closing
\ Should get two different file descriptors
\ Returns 1 if both fds are >= 0 and different, 0 otherwise
variable fd1
variable fd2
: main
  s" /dev/null" 0 open-file drop
  dup 0< if drop 0 exit then
  fd1 !
  s" /dev/null" 0 open-file drop
  dup 0< if drop 0 exit then
  fd2 !
  fd1 @ fd2 @ = if 0 else 1 then ;
