// expected: -511200
// Evaluate degree-100 polynomial, 100K times (using Horner's method)
// Checksum: sum of (result mod 1000) for each evaluation

#include <stdio.h>

long coeffs[101];

void init_coeffs(void) {
    for (int i = 0; i < 101; i++) {
        coeffs[i] = i + 1;
    }
}

long poly_eval(long x) {
    long result = 0;
    for (int i = 0; i < 101; i++) {
        result = result * x + coeffs[100 - i];
    }
    return result;
}

int main(void) {
    init_coeffs();
    long sum = 0;
    for (int i = 0; i < 100000; i++) {
        long r = poly_eval(i % 1000);
        sum += r % 1000;
    }
    printf("%ld\n", sum);
    return 0;
}
