// expected: 4953
// Abundant numbers benchmark - count abundant 1-20000, run 5 times

#include <stdio.h>

int proper_divisor_sum(int n) {
    if (n == 1) return 0;
    int sum = 1;
    for (int i = 2; i <= n / 2; i++) {
        if (n % i == 0) sum += i;
    }
    return sum;
}

int abundant(int n) {
    return proper_divisor_sum(n) > n;
}

int count_abundant(int limit) {
    int count = 0;
    for (int i = 1; i <= limit; i++) {
        if (abundant(i)) count++;
    }
    return count;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 5; i++) {
        result = count_abundant(20000);
    }
    printf("%d\n", result);
    return 0;
}
