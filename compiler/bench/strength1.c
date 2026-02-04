// expected: 800000000
// Strength reduction - multiply to shift
#include <stdio.h>

static long mul8(long n) { return n * 8; }

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        mul8(i);
        sum += 8;
    }
    printf("%ld\n", sum);
    return 0;
}
