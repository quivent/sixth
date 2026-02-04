// expected: 9227465
#include <stdio.h>
long fibrec(long n) {
    if (n < 2) return n;
    return fibrec(n - 1) + fibrec(n - 2);
}
int main() {
    printf("%ld\n", fibrec(35));
    return 0;
}
