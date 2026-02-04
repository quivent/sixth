// expected: 1388710083
// Lucas numbers - like Fibonacci but starts with 2, 1
#include <stdio.h>
#include <stdint.h>

long lucas(int n) {
    if (n == 0) return 2;
    if (n == 1) return 1;
    long a = 2, b = 1;
    for (int i = 1; i < n; i++) {
        long t = a + b;
        a = b;
        b = t;
    }
    return b;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 5000000; i++) {
        result ^= lucas(i % 45);
    }
    printf("%ld\n", result);
    return 0;
}
