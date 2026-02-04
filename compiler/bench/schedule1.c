// expected: 1300000000
// Instruction scheduling - independent ops
#include <stdio.h>

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        sum += 4 + 4 + 5;  // GCC computes at compile time
    }
    printf("%ld\n", sum);
    return 0;
}
