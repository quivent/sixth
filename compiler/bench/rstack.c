// expected: 499999999500000000
// Simulates return stack with local variable
#include <stdio.h>
int main() {
    long sum = 0;
    for (long i = 0; i < 1000000000; i++) {
        long r = i;   // >r
        sum += r;     // r@ +
        // r> drop implicit
    }
    printf("%ld\n", sum);
    return 0;
}
