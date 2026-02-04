// expected: 499996
// Digital root benchmark - sum digital roots 1-100000, run 100 times

#include <stdio.h>

int digit_sum(int n) {
    int sum = 0;
    while (n) {
        sum += n % 10;
        n /= 10;
    }
    return sum;
}

int digital_root(int n) {
    while (n > 9) {
        n = digit_sum(n);
    }
    return n;
}

long sum_roots(int limit) {
    long sum = 0;
    for (int i = 1; i <= limit; i++) {
        sum += digital_root(i);
    }
    return sum;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 100; i++) {
        result = sum_roots(100000);
    }
    printf("%ld\n", result);
    return 0;
}
