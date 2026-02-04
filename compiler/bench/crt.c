// expected: 1905079
// Chinese Remainder Theorem - solve system of modular equations
#include <stdio.h>
#include <stdint.h>

void ext_gcd(long a, long b, long *gcd, long *x, long *y) {
    if (b == 0) {
        *gcd = a;
        *x = 1;
        *y = 0;
        return;
    }
    long x1, y1;
    ext_gcd(b, a % b, gcd, &x1, &y1);
    *x = y1;
    *y = x1 - (a / b) * y1;
}

long mod_inv(long a, long m) {
    long gcd, x, y;
    ext_gcd(a, m, &gcd, &x, &y);
    return ((x % m) + m) % m;
}

long crt2(long r1, long m1, long r2, long m2) {
    long m = m1 * m2;
    long inv = mod_inv(m2, m1);
    long x = inv * (r1 - r2);
    long r = r2 + m2 * x;
    r = r % m;
    if (r < 0) r += m;
    return r;
}

int main(void) {
    long sum = 0;
    for (int i = 1; i < 100; i++) {
        for (int j = 1; j < 100; j++) {
            sum += crt2(i, 17, j, 23);
        }
    }
    printf("%ld\n", sum);
    return 0;
}
