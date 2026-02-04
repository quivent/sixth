// expected: 142
// Happy numbers benchmark - count happy numbers up to 1000, run 500 times

#include <stdio.h>

int sum_digit_squares(int n) {
    int sum = 0;
    while (n) {
        int d = n % 10;
        sum += d * d;
        n /= 10;
    }
    return sum;
}

int happy(int n) {
    for (int i = 0; i < 50; i++) {
        n = sum_digit_squares(n);
        if (n == 1) return 1;
    }
    return 0;
}

int count_happy(int limit) {
    int count = 0;
    for (int i = 1; i < limit; i++) {
        if (happy(i)) count++;
    }
    return count;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 500; i++) {
        result = count_happy(1000);
    }
    printf("%d\n", result);
    return 0;
}
