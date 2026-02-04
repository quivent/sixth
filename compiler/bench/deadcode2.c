// expected: 100000000
// Unreachable branch elimination - always false condition
#include <stdio.h>

static long process(long n) {
    if (n < 0) {
        return n * 1000000;  // never reached
    }
    return n;
}

int main() {
    long count = 0;
    for (long i = 0; i < 100000000; i++) {
        process(i);
        count++;
    }
    printf("%ld\n", count);
    return 0;
}
