// expected: 479001600
// Factorial benchmark - compute factorial of 12, 100000 times

#include <stdio.h>

long factorial(int n) {
    long result = 1;
    for (int i = 1; i <= n; i++) {
        result *= i;
    }
    return result;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 100000; i++) {
        result = factorial(12);
    }
    printf("%ld\n", result);
    return 0;
}
