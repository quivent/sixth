// expected: 9592
#include <stdio.h>
long isqrt(long n) {
    if (n < 2) return n;
    long guess = n;
    while (1) {
        long next = (guess + n / guess) / 2;
        if (next >= guess) return guess;
        guess = next;
    }
}
int is_prime(long n) {
    if (n < 2) return 0;
    if (n == 2) return 1;
    if (n % 2 == 0) return 0;
    long limit = isqrt(n);
    for (long d = 3; d <= limit; d += 2)
        if (n % d == 0) return 0;
    return 1;
}
int main() {
    long count = 0;
    for (long i = 2; i < 100000; i++)
        if (is_prime(i)) count++;
    printf("%ld\n", count);
    return 0;
}
