\ expect: 30
\ Many variables interacting
variable v1 variable v2 variable v3 variable v4 variable v5
: main
  1 v1 ! 2 v2 ! 3 v3 ! 4 v4 ! 5 v5 !
  v1 @ v2 @ + v3 @ + v4 @ + v5 @ +
  dup v1 !
  v1 @ 2 * v2 !
  v2 @ ;
