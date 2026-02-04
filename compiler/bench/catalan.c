// expected: 6564120420
#include <stdio.h>
long catalan(long n) {
    if (n <= 1) return 1;
    long sum = 0;
    for (long i = 0; i < n; i++) {
        sum += catalan(n - 1 - i) * catalan(i);
    }
    return sum;
}

int main() {
    printf("%ld\n", catalan(20));
    return 0;
}
