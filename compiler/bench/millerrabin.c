// expected: 9608
// Miller-Rabin primality test - count primes up to 100000
#include <stdio.h>
#include <stdint.h>

long modexp(long base, long exp, long mod) {
    long result = 1;
    base = base % mod;
    while (exp > 0) {
        if (exp & 1) result = (result * base) % mod;
        exp >>= 1;
        base = (base * base) % mod;
    }
    return result;
}

int miller_test(long n, long d, int r) {
    long x = modexp(2, d, n);
    if (x == 1 || x == n - 1) return 1;
    for (int i = 0; i < r - 1; i++) {
        x = (x * x) % n;
        if (x == n - 1) return 1;
    }
    return 0;
}

int is_prime_mr(long n) {
    if (n < 2) return 0;
    if (n == 2) return 1;
    if ((n & 1) == 0) return 0;
    long d = n - 1;
    int r = 0;
    while ((d & 1) == 0) { d >>= 1; r++; }
    return miller_test(n, d, r);
}

int main(void) {
    long count = 0;
    for (long i = 2; i < 100000; i++) {
        if (is_prime_mr(i)) count++;
    }
    printf("%ld\n", count);
    return 0;
}
