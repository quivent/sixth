// expected: 7
#include <stdio.h>
long tak(long x, long y, long z) {
    if (y >= x) return z;
    return tak(tak(x-1, y, z), tak(y-1, z, x), tak(z-1, x, y));
}
int main() {
    printf("%ld\n", tak(18, 12, 6));
    return 0;
}
