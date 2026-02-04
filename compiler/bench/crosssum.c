// expected: 315000000
// Sum of digits, 10M numbers
// Checksum: sum of all digit sums

#include <stdio.h>

long digit_sum(long n) {
    long sum = 0;
    while (n > 0) {
        sum += n % 10;
        n /= 10;
    }
    return sum;
}

int main(void) {
    long sum = 0;
    for (long i = 0; i < 10000000; i++) {
        sum += digit_sum(i);
    }
    printf("%ld\n", sum);
    return 0;
}
