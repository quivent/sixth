// expected: 1000000
#include <stdio.h>
long rec100_helper(long n) {
    if (n == 0) return 0;
    return rec100_helper(n - 1) + 1;
}
int main() {
    long sum = 0;
    for (long i = 0; i < 10000; i++) {
        sum += rec100_helper(100);
    }
    printf("%ld\n", sum);
    return 0;
}
