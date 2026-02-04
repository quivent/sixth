// expected: 100000000
// Tail call optimization - GCC converts to loop
#include <stdio.h>

long countdown(long n, long acc) {
    if (n == 0) return acc;
    return countdown(n - 1, acc + 1);
}

int main() {
    printf("%ld\n", countdown(100000000, 0));
    return 0;
}
