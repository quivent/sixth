// expected: 1000000000
#include <stdio.h>
__attribute__((noinline)) long inc1(long n) { return n + 1; }
int main() {
    long n = 0;
    for (long i = 0; i < 1000000000; i++)
        n = inc1(n);
    printf("%ld\n", n);
    return 0;
}
