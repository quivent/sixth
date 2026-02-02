\ expected: 7

: tak ( x y z -- result )
  over 2 pick > if
    2 pick 1- 2 pick 2 pick recurse
    3 pick 1- 3 pick 5 pick recurse
    3 pick 1- 5 pick 5 pick recurse
    recurse nip nip nip
  else nip nip then ;

: main 18 12 6 tak . cr ;
