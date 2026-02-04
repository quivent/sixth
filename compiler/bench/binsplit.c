// expected: 1048576
#include <stdio.h>
long binsplit(long depth) {
    if (depth == 0) return 1;
    return binsplit(depth - 1) + binsplit(depth - 1);
}

int main() {
    for (long i = 0; i < 1000; i++) {
        binsplit(20);
    }
    printf("%ld\n", binsplit(20));
    return 0;
}
