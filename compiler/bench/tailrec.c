// expected: 100000000
#include <stdio.h>
long tailrec(long n, long acc) {
    if (n == 0) return acc;
    return tailrec(n - 1, acc + 1);
}
int main() {
    printf("%ld\n", tailrec(100000000, 0));
    return 0;
}
