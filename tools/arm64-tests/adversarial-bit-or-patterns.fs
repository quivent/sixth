\ Adversarial test: OR combining bit patterns
\ 0xF0F0F0F0 OR 0x0F0F0F0F = 0xFFFFFFFF (lower 32 bits)
\ Using smaller values: 240 (0xF0) OR 15 (0x0F) = 255 (0xFF)
\ expect: 255
: main
  240 15 or   \ 0xF0 OR 0x0F = 0xFF
;
