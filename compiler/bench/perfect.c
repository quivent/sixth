// expected: 4
// Perfect numbers benchmark - count perfect numbers up to 10000, 100 times

#include <stdio.h>

int sum_divisors(int n) {
    int sum = 1;
    for (int i = 2; i <= n / 2; i++) {
        if (n % i == 0) sum += i;
    }
    return sum;
}

int perfect(int n) {
    return sum_divisors(n) == n;
}

int count_perfect(int limit) {
    int count = 0;
    for (int i = 2; i < limit; i++) {
        if (perfect(i)) count++;
    }
    return count;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 100; i++) {
        result = count_perfect(10000);
    }
    printf("%d\n", result);
    return 0;
}
