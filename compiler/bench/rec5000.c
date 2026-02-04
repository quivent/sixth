// expected: 50000
#include <stdio.h>
long rec5000_helper(long n) {
    if (n == 0) return 0;
    return rec5000_helper(n - 1) + 1;
}
int main() {
    long sum = 0;
    for (long i = 0; i < 10; i++) {
        sum += rec5000_helper(5000);
    }
    printf("%ld\n", sum);
    return 0;
}
