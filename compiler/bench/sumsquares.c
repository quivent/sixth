// expected: 333833500
// Sum of squares benchmark - sum of squares 1-1000, run 10000 times

#include <stdio.h>

long sum_squares(int n) {
    long sum = 0;
    for (int i = 1; i <= n; i++) {
        sum += (long)i * i;
    }
    return sum;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 10000; i++) {
        result = sum_squares(1000);
    }
    printf("%ld\n", result);
    return 0;
}
