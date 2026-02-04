// expected: 49995000
// Sum array - pattern GCC can auto-vectorize
#include <stdio.h>
#define SIZE 10000
long arr[SIZE];

int main() {
    for (int i = 0; i < SIZE; i++) arr[i] = i;
    long sum = 0;
    for (int i = 0; i < SIZE; i++) sum += arr[i];
    printf("%ld\n", sum);
    return 0;
}
