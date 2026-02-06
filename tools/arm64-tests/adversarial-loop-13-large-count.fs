\ adversarial-loop-13-large-count.fs - Very large loop counts (1000+)
\ Count to 1000, result mod 256 = 232
\ expect: 232

: main
  0
  1000 0 do
    1+
  loop
  255 and  \ return only low byte for exit code
;
