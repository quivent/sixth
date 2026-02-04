// expected: 46368
// Fibonacci benchmark - iterative fib(24) run 50000 times

#include <stdio.h>

long fib(int n) {
    if (n < 2) return n;
    long a = 0, b = 1;
    for (int i = 1; i < n; i++) {
        long tmp = a + b;
        a = b;
        b = tmp;
    }
    return b;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 50000; i++) {
        result = fib(24);
    }
    printf("%ld\n", result);
    return 0;
}
