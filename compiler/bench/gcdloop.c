// expected: 1286500000
// GCD of 10M pairs
// Checksum: sum of all GCDs

#include <stdio.h>

long gcd(long a, long b) {
    while (b) {
        long t = b;
        b = a % b;
        a = t;
    }
    return a;
}

int main(void) {
    long sum = 0;
    for (long i = 0; i < 10000000; i++) {
        sum += gcd((i % 1000) + 1, (i % 500) + 1);
    }
    printf("%ld\n", sum);
    return 0;
}
