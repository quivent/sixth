// expected: 10000000
#include <stdio.h>
long g2(long n);
long f2(long n) {
    if (n == 0) return 0;
    return g2(n - 1) + 1;
}
long g2(long n) {
    if (n == 0) return 0;
    return f2(n - 1) + 1;
}
int main() {
    long sum = 0;
    for (long i = 0; i < 100000; i++) {
        sum += f2(100);
    }
    printf("%ld\n", sum);
    return 0;
}
