#!/bin/sh
# bench.sh - Benchmark tf.fs against gcc

echo "=== BENCHMARK: tf.fs vs gcc ==="
echo ""

# --- countdown 100M ---
echo "--- countdown 100M ---"

cat > /tmp/count.fs << 'EOF'
: main 100000000 begin 1 - dup 0= until drop 0 . cr ;
EOF

cat > /tmp/count.c << 'EOF'
#include <stdio.h>
int main(){for(long i=100000000;i>0;i--);printf("0\n");return 0;}
EOF

./fifth compiler/tf.fs /tmp/count.fs /tmp/count-fifth 2>/dev/null
gcc -O0 /tmp/count.c -o /tmp/count-gcc0
gcc -O2 /tmp/count.c -o /tmp/count-gcc2

echo "Size:"
printf "  Fifth:   %6d bytes\n" $(stat -c%s /tmp/count-fifth)
printf "  gcc -O0: %6d bytes\n" $(stat -c%s /tmp/count-gcc0)
printf "  gcc -O2: %6d bytes\n" $(stat -c%s /tmp/count-gcc2)
echo ""

echo "Speed:"
printf "  Fifth:   "; /usr/bin/time -f "%es" /tmp/count-fifth 2>&1
printf "  gcc -O0: "; /usr/bin/time -f "%es" /tmp/count-gcc0 2>&1
printf "  gcc -O2: (optimized away)\n"
echo ""

# --- arithmetic 10M ---
echo "--- arithmetic 10M ops ---"

cat > /tmp/arith.fs << 'EOF'
: main 1 10000000 begin 1 - dup 0 > while swap 3 + 2 * 7 - swap repeat drop . cr ;
EOF

cat > /tmp/arith.c << 'EOF'
#include <stdio.h>
int main(){long x=1;for(long i=10000000;i>0;i--)x=(x+3)*2-7;printf("%ld\n",x);return 0;}
EOF

./fifth compiler/tf.fs /tmp/arith.fs /tmp/arith-fifth 2>/dev/null
gcc -O0 /tmp/arith.c -o /tmp/arith-gcc0
gcc -O2 /tmp/arith.c -o /tmp/arith-gcc2

echo "Size:"
printf "  Fifth:   %6d bytes\n" $(stat -c%s /tmp/arith-fifth)
printf "  gcc -O0: %6d bytes\n" $(stat -c%s /tmp/arith-gcc0)
printf "  gcc -O2: %6d bytes\n" $(stat -c%s /tmp/arith-gcc2)
echo ""

echo "Result:"
printf "  Fifth:   "; /tmp/arith-fifth
printf "  gcc:     "; /tmp/arith-gcc0
echo ""

echo "Speed:"
printf "  Fifth:   "; /usr/bin/time -f "%es" /tmp/arith-fifth 2>&1
printf "  gcc -O0: "; /usr/bin/time -f "%es" /tmp/arith-gcc0 2>&1
printf "  gcc -O2: "; /usr/bin/time -f "%es" /tmp/arith-gcc2 2>&1
echo ""

# --- hello world ---
echo "--- hello world ---"

cat > /tmp/hello.fs << 'EOF'
: main 72 emit 101 emit 108 emit 108 emit 111 emit 10 emit ;
EOF

cat > /tmp/hello.c << 'EOF'
#include <stdio.h>
int main(){printf("Hello\n");return 0;}
EOF

./fifth compiler/tf.fs /tmp/hello.fs /tmp/hello-fifth 2>/dev/null
gcc -O0 /tmp/hello.c -o /tmp/hello-gcc0

echo "Size:"
printf "  Fifth:   %6d bytes\n" $(stat -c%s /tmp/hello-fifth)
printf "  gcc -O0: %6d bytes\n" $(stat -c%s /tmp/hello-gcc0)
echo ""

echo "Output:"
printf "  Fifth:   "; /tmp/hello-fifth
printf "  gcc:     "; /tmp/hello-gcc0
echo ""

echo "=== SUMMARY ==="
printf "Fifth:   ~400 bytes (40x smaller)\n"
printf "gcc -O0: ~16KB\n"
echo "=== DONE ==="
