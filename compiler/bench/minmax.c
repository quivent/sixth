// expected: 65536
// Find min and max benchmark - 1M elements

#include <stdio.h>

#define SIZE 1000000

int arr[SIZE];

void find_minmax(int *min_out, int *max_out) {
    int min = arr[0], max = arr[0];
    for (int i = 1; i < SIZE; i++) {
        if (arr[i] < min) min = arr[i];
        else if (arr[i] > max) max = arr[i];
    }
    *min_out = min;
    *max_out = max;
}

void init_arr(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = ((long)i * 31337) % 65537;
    }
}

int main(void) {
    init_arr();
    int min, max;
    find_minmax(&min, &max);
    printf("%d\n", min + max);
    return 0;
}
