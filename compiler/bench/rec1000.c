// expected: 100000
#include <stdio.h>
long rec1000_helper(long n) {
    if (n == 0) return 0;
    return rec1000_helper(n - 1) + 1;
}
int main() {
    long sum = 0;
    for (long i = 0; i < 100; i++) {
        sum += rec1000_helper(1000);
    }
    printf("%ld\n", sum);
    return 0;
}
