// expected: 823739198976
// Polynomial evaluation unroll pattern - Horner's method
#include <stdio.h>

long poly(long x) {
    return 1 + x + 2*x*x + 3*x*x*x;
}

int main() {
    long sum = 0;
    for (int i = 0; i < 1024; i++) sum += poly(i);
    printf("%ld\n", sum);
    return 0;
}
