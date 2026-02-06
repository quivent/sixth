// Ackermann(3,10) benchmark - deep recursion
#include <stdio.h>

long ackermann(long m, long n) {
    if (m == 0) return n + 1;
    if (n == 0) return ackermann(m - 1, 1);
    return ackermann(m - 1, ackermann(m, n - 1));
}

int main() {
    long result = ackermann(3, 10);
    printf("%ld\n", result);
    return 0;
}
