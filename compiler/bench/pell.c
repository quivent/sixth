// expected: 0
// Pell numbers - P(n) = 2*P(n-1) + P(n-2)
#include <stdio.h>
#include <stdint.h>

long pell(int n) {
    if (n == 0) return 0;
    if (n == 1) return 1;
    long a = 0, b = 1;
    for (int i = 1; i < n; i++) {
        long t = 2 * b + a;
        a = b;
        b = t;
    }
    return b;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 5000000; i++) {
        result ^= pell(i % 32);
    }
    printf("%ld\n", result);
    return 0;
}
