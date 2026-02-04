// expected: 6020697
// Pollard's rho - find factors using cycle detection
#include <stdio.h>
#include <stdlib.h>

long gcd(long a, long b) {
    while (b) { long t = b; b = a % b; a = t; }
    return a;
}

long pollard_f(long x, long n) {
    return (x * x + 1) % n;
}

long pollard_rho(long n) {
    if ((n & 1) == 0) return 2;
    long x = 2, y = 2;
    long d = 1;
    while (d == 1) {
        x = pollard_f(x, n);
        y = pollard_f(pollard_f(y, n), n);
        d = gcd(labs(x - y), n);
    }
    return d;
}

int main(void) {
    long sum = 0;
    for (long i = 3; i < 10000; i += 2) {
        sum += pollard_rho(i);
    }
    printf("%ld\n", sum);
    return 0;
}
