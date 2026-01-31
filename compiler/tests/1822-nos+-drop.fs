\ expect: 8
\ nos+ then drop TOS to get incremented NOS
\ Stack: 7 3 -> nos+ -> ( 8 3 ) -> drop -> ( 8 )
: main 7 3 nos+ drop . cr ;
