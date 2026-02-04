// expected: 9999
// Min/max reduction - GCC vectorizes reductions
#include <stdio.h>
#define SIZE 10000
long arr[SIZE];

int main() {
    for (int i = 0; i < SIZE; i++) arr[i] = SIZE - i - 1;
    long mx = 0;
    for (int i = 0; i < SIZE; i++) if (arr[i] > mx) mx = arr[i];
    printf("%ld\n", mx);
    return 0;
}
