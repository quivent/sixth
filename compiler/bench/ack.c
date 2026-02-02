// Ackermann benchmark: ack(3,10)=8189, ack(4,1)=65533
#include <stdio.h>
long ack(long m, long n) {
    if (m == 0) return n + 1;
    if (n == 0) return ack(m - 1, 1);
    return ack(m - 1, ack(m, n - 1));
}
int main() {
    printf("%ld\n", ack(3, 10));
    printf("%ld\n", ack(4, 1));
    return 0;
}
