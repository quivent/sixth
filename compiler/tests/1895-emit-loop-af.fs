\ expect: ABCDEF
\ Emit A(65) through F(70) using a loop
: main 6 0 do 65 i + emit loop cr ;
