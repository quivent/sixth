// expected: 78498
// Primality test, 1M numbers
// Checksum: count of primes found

#include <stdio.h>

int is_prime(long n) {
    if (n < 2) return 0;
    if (n == 2) return 1;
    if (n % 2 == 0) return 0;
    if (n == 3) return 1;
    if (n % 3 == 0) return 0;
    long i = 5;
    while (i * i <= n) {
        if (n % i == 0) return 0;
        i += 2;
        if (n % i == 0) return 0;
        i += 4;
    }
    return 1;
}

int main(void) {
    long count = 0;
    for (long i = 0; i < 1000000; i++) {
        count += is_prime(i);
    }
    printf("%ld\n", count);
    return 0;
}
