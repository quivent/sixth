// expected: 8189
#include <stdio.h>
long ackmed(long m, long n) {
    if (m == 0) return n + 1;
    if (n == 0) return ackmed(m - 1, 1);
    return ackmed(m - 1, ackmed(m, n - 1));
}
int main() {
    printf("%ld\n", ackmed(3, 10));
    return 0;
}
