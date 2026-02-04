// expected: 1229
// Primality by trial division - count primes to 10000, run 100 times

#include <stdio.h>

int prime(int n) {
    if (n < 2) return 0;
    if (n == 2) return 1;
    if ((n & 1) == 0) return 0;
    for (int i = 3; i * i <= n; i += 2) {
        if (n % i == 0) return 0;
    }
    return 1;
}

int count_primes(int limit) {
    int count = 0;
    for (int i = 2; i < limit; i++) {
        if (prime(i)) count++;
    }
    return count;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 100; i++) {
        result = count_primes(10000);
    }
    printf("%d\n", result);
    return 0;
}
