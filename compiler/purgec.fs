\ purgec.fs - Find C code to eliminate
\ Usage: fifth compiler/purgec.fs

s" echo '=== PURGE C ==='" system
s" echo 'Goal: Forth compiles Forth to machine code. No C.'" system
s" echo ''" system
s" echo 'C files:'" system
s" find . -name '*.c' | grep -v .git | sort" system
s" echo ''" system
s" echo 'Lines of C:'" system
s" find . -name '*.c' | grep -v .git | xargs wc -l | tail -1" system
s" echo ''" system
s" echo 'Engine (to eliminate):'" system
s" wc -l engine/*.c engine/*.h" system
s" echo ''" system
s" echo 'Native compiler (pure Forth):'" system
s" wc -l compiler/tf.fs compiler/ff.fs" system
s" echo ''" system
s" echo '=== PLAN ==='" system
s" echo '1. tf.fs compiles itself -> native tf'" system
s" echo '2. native tf compiles programs -> no C needed'" system
s" echo '3. Delete engine/'" system

bye
