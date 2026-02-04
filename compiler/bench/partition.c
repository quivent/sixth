// expected: 190569292
#include <stdio.h>
long partition(long n, long max) {
    if (n == 0) return 1;
    if (n < 0) return 0;
    if (max == 0) return 0;
    return partition(n - max, max) + partition(n, max - 1);
}

int main() {
    printf("%ld\n", partition(100, 100));
    return 0;
}
