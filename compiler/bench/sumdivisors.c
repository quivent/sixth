// expected: 82256014
// Sum of divisors benchmark - sum divisor sums 1-10000, run 10 times

#include <stdio.h>

int divisor_sum(int n) {
    if (n == 1) return 1;
    int sum = 1 + n;
    for (int i = 2; i <= n / 2; i++) {
        if (n % i == 0) sum += i;
    }
    return sum;
}

long sum_all_divisors(int limit) {
    long sum = 0;
    for (int i = 1; i <= limit; i++) {
        sum += divisor_sum(i);
    }
    return sum;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 10; i++) {
        result = sum_all_divisors(10000);
    }
    printf("%ld\n", result);
    return 0;
}
