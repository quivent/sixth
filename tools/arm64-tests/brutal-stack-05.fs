\ expect: 0
\ Test: NIP removes second item (a b -- b)
\ TUCK puts TOS under second (a b -- b a b)
\ Combined: a b tuck nip nip = b

: main
  999 777
  tuck        ( Stack: 777 999 777 )
  nip         ( Stack: 777 777 )
  nip         ( Stack: 777 )
  777 -
;
