// expected: depends on init
#include <stdio.h>
#define SIZE 4096
long buf[SIZE];
int main() {
    for (int i = 0; i < SIZE; i++) buf[i] = i;
    long sum = 0;
    for (long i = 0; i < 1000000000; i++)
        sum += buf[i & 0xFFF];
    printf("%ld\n", sum);
    return 0;
}
