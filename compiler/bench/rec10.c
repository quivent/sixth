// expected: 10000000
#include <stdio.h>
long rec10_helper(long n) {
    if (n == 0) return 0;
    return rec10_helper(n - 1) + 1;
}
int main() {
    long sum = 0;
    for (long i = 0; i < 1000000; i++) {
        sum += rec10_helper(10);
    }
    printf("%ld\n", sum);
    return 0;
}
