// expected: 666566670000
// Parallel multiply-add - vectorizable fused multiply-add
#include <stdio.h>
#define SIZE 10000
long a[SIZE], b[SIZE];

int main() {
    for (int i = 0; i < SIZE; i++) { a[i] = i; b[i] = i * 2; }
    long sum = 0;
    for (int i = 0; i < SIZE; i++) sum += a[i] * b[i];
    printf("%ld\n", sum);
    return 0;
}
