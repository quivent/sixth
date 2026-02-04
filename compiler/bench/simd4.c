// expected: 3334
// Count matching elements - vectorizable conditional count
#include <stdio.h>
#define SIZE 10000
long arr[SIZE];

int main() {
    for (int i = 0; i < SIZE; i++) arr[i] = i % 3;
    long count = 0;
    for (int i = 0; i < SIZE; i++) if (arr[i] == 0) count++;
    printf("%ld\n", count);
    return 0;
}
