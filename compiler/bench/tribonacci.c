// expected: 989640352
// Tribonacci sequence - each term is sum of previous three
#include <stdio.h>
#include <stdint.h>

long tribonacci(int n) {
    if (n == 0) return 0;
    if (n == 1) return 0;
    if (n == 2) return 1;
    long a = 0, b = 0, c = 1;
    for (int i = 2; i < n; i++) {
        long t = a + b + c;
        a = b; b = c; c = t;
    }
    return c;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 5000000; i++) {
        result ^= tribonacci(i % 37);
    }
    printf("%ld\n", result);
    return 0;
}
