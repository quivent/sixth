// expected: 31626
// Amicable pairs benchmark - sum of amicable pairs below 10000, run 5 times

#include <stdio.h>

int proper_divisor_sum(int n) {
    if (n == 1) return 0;
    int sum = 1;
    for (int i = 2; i <= n / 2; i++) {
        if (n % i == 0) sum += i;
    }
    return sum;
}

long amicable_sum(int limit) {
    long sum = 0;
    for (int i = 2; i < limit; i++) {
        int s = proper_divisor_sum(i);
        if (s > i && proper_divisor_sum(s) == i) {
            sum += i + s;
        }
    }
    return sum;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 5; i++) {
        result = amicable_sum(10000);
    }
    printf("%ld\n", result);
    return 0;
}
