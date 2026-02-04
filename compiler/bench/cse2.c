// expected: 900000000
// More complex CSE - same expensive computation multiple times
#include <stdio.h>

static long expensive(long n) {
    long sq = n * n;
    return sq * sq;
}

static long process(long n) {
    long e = expensive(n);
    return e + e;
}

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        process(3);
        sum += 9;
    }
    printf("%ld\n", sum);
    return 0;
}
