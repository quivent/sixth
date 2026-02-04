// expected: 1100000000
// Common subexpression elimination - repeated computation
#include <stdio.h>

static long compute(long a, long b) {
    return (a + b) + (a * b);
}

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        sum += compute(2, 3);
    }
    printf("%ld\n", sum);
    return 0;
}
