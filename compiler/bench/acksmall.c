// expected: 2045
#include <stdio.h>
long acksmall(long m, long n) {
    if (m == 0) return n + 1;
    if (n == 0) return acksmall(m - 1, 1);
    return acksmall(m - 1, acksmall(m, n - 1));
}
int main() {
    printf("%ld\n", acksmall(3, 8));
    return 0;
}
