// expected: 100000000
// Dead code elimination - unused computation
#include <stdio.h>

static long useless(long n) {
    long x = n * 1000;  // dead
    (void)x;
    return n;
}

int main() {
    long count = 0;
    for (long i = 0; i < 100000000; i++) {
        useless(i);
        count++;
    }
    printf("%ld\n", count);
    return 0;
}
