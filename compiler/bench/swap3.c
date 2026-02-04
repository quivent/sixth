// expected: 300000000
// 3-way rotate via swaps, 300M times
#include <stdio.h>

int main(void) {
    long a = 0, b = 1, c = 2;
    long tmp;
    for (long i = 0; i < 300000000L; i++) {
        // swap a,b then swap b,c = rot
        tmp = a; a = b; b = tmp;
        tmp = b; b = c; c = tmp;
        // rot again
        tmp = a; a = b; b = tmp;
        tmp = b; b = c; c = tmp;
        // rot again (back to original)
        tmp = a; a = b; b = tmp;
        tmp = b; b = c; c = tmp;
        a = a + 1;
    }
    printf("%ld\n", a);
    return 0;
}
