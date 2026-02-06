\ expect: 78
\ ADVERSARIAL: Memory fetch operations in loops
\ Tests repeated @ operations maintaining correct values
\ Sum of 0+1+2+...+99 = 4950, mod 256 = 78

variable val
variable sum

: main
  0 sum !
  100 0 do
    i val !               \ store i
    val @ sum @ + sum !   \ sum += val (tests both @ and !)
  loop
  sum @                   \ should be 4950
  4950 = if 78 else 0 then  \ return 78 if correct
;
