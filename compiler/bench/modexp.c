// expected: 243590549
// Modular exponentiation - compute base^exp mod m
#include <stdio.h>
#include <stdint.h>

long modexp(long base, long exp, long mod) {
    long result = 1;
    base = base % mod;
    while (exp > 0) {
        if (exp & 1) {
            result = (result * base) % mod;
        }
        exp >>= 1;
        base = (base * base) % mod;
    }
    return result;
}

int main(void) {
    long result = 0;
    for (int i = 1; i < 1000; i++) {
        for (int j = 1; j < 1000; j++) {
            result ^= modexp(i, j, 1000000007);
        }
    }
    printf("%ld\n", result);
    return 0;
}
