// expected: 20
// Narcissistic numbers benchmark - count narcissistic 1-1000000, run 5 times

#include <stdio.h>

int count_digits(int n) {
    int count = 0;
    while (n) { n /= 10; count++; }
    return count;
}

long ipow(int base, int exp) {
    if (exp == 0) return 1;
    long result = 1;
    for (int i = 0; i < exp; i++) {
        result *= base;
    }
    return result;
}

int narcissistic(int n) {
    int digits = count_digits(n);
    long sum = 0;
    int tmp = n;
    while (tmp) {
        int d = tmp % 10;
        sum += ipow(d, digits);
        tmp /= 10;
    }
    return sum == n;
}

int count_narcissistic(int limit) {
    int count = 0;
    for (int i = 1; i <= limit; i++) {
        if (narcissistic(i)) count++;
    }
    return count;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 5; i++) {
        result = count_narcissistic(1000000);
    }
    printf("%d\n", result);
    return 0;
}
