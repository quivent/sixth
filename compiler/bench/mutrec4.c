// expected: 2000000
#include <stdio.h>
long g4(long n);
long h4(long n);
long i4(long n);
long f4(long n) {
    if (n == 0) return 0;
    return g4(n - 1) + 1;
}
long g4(long n) {
    if (n == 0) return 0;
    return h4(n - 1) + 1;
}
long h4(long n) {
    if (n == 0) return 0;
    return i4(n - 1) + 1;
}
long i4(long n) {
    if (n == 0) return 0;
    return f4(n - 1) + 1;
}
int main() {
    long sum = 0;
    for (long i = 0; i < 20000; i++) {
        sum += f4(100);
    }
    printf("%ld\n", sum);
    return 0;
}
