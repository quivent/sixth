\ Adversarial test for pick word
\ pick ( xu ... x1 x0 u -- xu ... x1 x0 xu ) copies the uth item to top of stack

\ 0 pick is equivalent to dup
10 0 pick
. .
\ Expected output: 10 10

\ 1 pick is equivalent to over
20 30 1 pick
. . .
\ Expected output: 20 30 20

\ 2 pick copies the third item
100 200 300 2 pick
. . . .
\ Expected output: 100 300 200 100

\ 3 pick copies the fourth item
1 2 3 4 3 pick
. . . . .
\ Expected output: 1 4 3 2 1

\ Verify original stack unchanged except for new top
\ Stack before: 5 6 7
\ After 1 pick: 5 6 7 6
5 6 7 1 pick
. . . .
\ Expected output: 6 7 6 5

\ Multiple picks in sequence
40 50 60 70
0 pick .
\ Expected output: 70
2 pick .
\ Expected output: 50
drop drop drop drop

\ 0 pick on single item
99 0 pick
. .
\ Expected output: 99 99

\ Deep pick - 4 pick copies fifth item
11 22 33 44 55 4 pick
. . . . . .
\ Expected output: 11 55 44 33 22 11

\ REMINDER: pick copies, does not remove. 0 pick = dup. 1 pick = over.
