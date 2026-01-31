\ expect: 1024
: power ( base exp -- result )
  1 swap 0 do over * loop nip ;
: main 2 10 power . cr ;
