// expected: 50000005000000
// Median of 3 benchmark - 10M iterations

#include <stdio.h>

#define ITERS 10000000

int median3(int a, int b, int c) {
    if (a > b) { int t = a; a = b; b = t; }
    if (b > c) { int t = b; b = c; c = t; }
    if (a > b) { int t = a; a = b; b = t; }
    return b;
}

long bench(void) {
    long sum = 0;
    for (int i = 0; i < ITERS; i++) {
        sum += median3(i, i + 1, i + 2);
    }
    return sum;
}

int main(void) {
    printf("%ld\n", bench());
    return 0;
}
