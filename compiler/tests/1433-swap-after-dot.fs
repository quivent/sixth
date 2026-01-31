\ expect: 30 10
\ Test 1433: swap after . — verify NOS survives IO (depth tracking after print)
\ 10 20 30 → . prints 30 → stack: 10 20 → swap → 20 10
\ . prints 10? No wait: swap makes it 20 10, TOS=10. . prints 10.
\ Then . prints 20. Output: 30 10 20
\ Hmm 3 values, 3 prints.
\ Let me simplify: 10 20 30 . swap . drop
\ . prints 30 → 10 20 → swap → 20 10 → . prints 10 → 20 → drop → empty
\ Output: 30 10
: main 10 20 30 . swap . drop cr ;
