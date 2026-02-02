\ Test: unsigned comparison words u< and u>
\ Adversarial test for unsigned comparison semantics

\ u< with two positive numbers
7 10 u< assert
10 7 u< invert assert

\ u< where signed would give wrong answer
\ -1 is MAX-UINT (all bits set), so 5 u< -1 should be true
5 -1 u< assert
0 -1 u< assert

\ u> with two positive numbers
10 7 u> assert
7 10 u> invert assert

\ u> where signed would give wrong answer
\ -1 is MAX-UINT, so -1 u> 5 should be true
-1 5 u> assert
-1 0 u> assert

\ boundary cases with 0
0 1 u< assert
1 0 u> assert
0 0 u< invert assert
0 0 u> invert assert

\ boundary cases with -1 (all bits set = MAX-UINT)
-1 -1 u< invert assert
-1 -1 u> invert assert

\ -2 is MAX-UINT minus 1, so -1 u> -2 should be true
-1 -2 u> assert
-2 -1 u< assert

\ mixed negative numbers treated as large unsigned
\ -1 = MAX, -2 = MAX-1, etc.
-100 -1 u< assert
-1 -100 u> assert

\ REMINDER: Unsigned comparison treats all bits as magnitude. -1 is the largest number.
