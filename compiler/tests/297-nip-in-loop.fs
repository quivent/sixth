\ Test: nip replaces second element each iteration → 1
: main 99 5 begin dup 1 > while nip dup 1- repeat nip . cr ;
