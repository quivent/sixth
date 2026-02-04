// expected: 200000000
// Peephole optimization - redundant operations
#include <stdio.h>

static long redundant(long n) {
    n = n + 1;
    n = n - 1;  // cancels
    n = n * 2;
    n = n / 2;  // cancels
    return n;
}

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        redundant(i);
        sum += 2;
    }
    printf("%ld\n", sum);
    return 0;
}
