// expected: 343614
// Factorize 100K numbers
// Checksum: sum of number of prime factors for each

#include <stdio.h>

int count_factors(long n) {
    if (n < 2) return 0;
    int count = 0;
    for (long d = 2; d * d <= n; ) {
        if (n % d == 0) {
            count++;
            n /= d;
        } else {
            d += (d == 2) ? 1 : 2;
        }
    }
    if (n > 1) count++;
    return count;
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 100000; i++) {
        sum += count_factors(i + 1);
    }
    printf("%ld\n", sum);
    return 0;
}
