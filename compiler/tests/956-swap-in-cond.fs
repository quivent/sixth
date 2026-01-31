\ Test 956: swap in condition for begin/until
\ Two counters: count down outer while accumulating sum
: main 0 3 begin dup . swap over + swap 1- dup 0= until drop . cr ;
