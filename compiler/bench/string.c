// expected: 255
#include <stdio.h>
#include <string.h>
#define SIZE 4096
char src[SIZE], dst[SIZE];
int main() {
    memset(src, 255, SIZE);
    for (long i = 0; i < 100000; i++) {
        memcpy(dst, src, SIZE);
        memcpy(src, dst, SIZE);
    }
    printf("%d\n", (unsigned char)dst[0]);
    return 0;
}
