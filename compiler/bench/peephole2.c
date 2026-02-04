// expected: 500000000
// Instruction combining - GCC combines ops
#include <stdio.h>

static long combine(long n) {
    n = n + 3;
    n = n + 2;  // GCC combines to n + 5
    return n;
}

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        combine(i);
        sum += 5;
    }
    printf("%ld\n", sum);
    return 0;
}
