// expected: 15043
// Deficient numbers benchmark - count deficient 1-20000, run 5 times

#include <stdio.h>

int proper_divisor_sum(int n) {
    if (n == 1) return 0;
    int sum = 1;
    for (int i = 2; i <= n / 2; i++) {
        if (n % i == 0) sum += i;
    }
    return sum;
}

int deficient(int n) {
    return proper_divisor_sum(n) < n;
}

int count_deficient(int limit) {
    int count = 0;
    for (int i = 1; i <= limit; i++) {
        if (deficient(i)) count++;
    }
    return count;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 5; i++) {
        result = count_deficient(20000);
    }
    printf("%d\n", result);
    return 0;
}
