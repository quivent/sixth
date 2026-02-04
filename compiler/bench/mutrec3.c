// expected: 5000000
#include <stdio.h>
long g3(long n);
long h3(long n);
long f3(long n) {
    if (n == 0) return 0;
    return g3(n - 1) + 1;
}
long g3(long n) {
    if (n == 0) return 0;
    return h3(n - 1) + 1;
}
long h3(long n) {
    if (n == 0) return 0;
    return f3(n - 1) + 1;
}
int main() {
    long sum = 0;
    for (long i = 0; i < 50000; i++) {
        sum += f3(100);
    }
    printf("%ld\n", sum);
    return 0;
}
