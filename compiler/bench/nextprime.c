// expected: 496267742
// Find next prime, 100K times
// Checksum: sum of all next primes (mod 10000)

#include <stdio.h>

int is_prime(long n) {
    if (n < 2) return 0;
    if (n == 2) return 1;
    if (n % 2 == 0) return 0;
    for (long i = 3; i * i <= n; i += 2) {
        if (n % i == 0) return 0;
    }
    return 1;
}

long next_prime(long n) {
    n++;
    if (n % 2 == 0) n++;
    while (!is_prime(n)) n += 2;
    return n;
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 100000; i++) {
        sum += next_prime(i * 100) % 10000;
    }
    printf("%ld\n", sum);
    return 0;
}
