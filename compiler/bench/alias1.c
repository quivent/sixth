// expected: 200000
// No aliasing case - GCC can optimize independent arrays
#include <stdio.h>
#define SIZE 1024

long arr1[SIZE], arr2[SIZE];

int main() {
    for (int i = 0; i < SIZE; i++) { arr1[i] = i; arr2[i] = i * 2; }
    long sum = 0;
    for (int j = 0; j < 100000; j++) {
        for (int i = 0; i < SIZE; i++) {
            (void)arr1[i];
            (void)arr2[i];
        }
        sum += 2;
    }
    printf("%ld\n", sum);
    return 0;
}
