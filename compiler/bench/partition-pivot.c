// expected: 499016000
// Partition around pivot benchmark - 1M iterations

#include <stdio.h>

#define SIZE 1000
#define ITERS 1000000

int arr[SIZE];

void swap_arr(int i, int j) {
    int tmp = arr[i];
    arr[i] = arr[j];
    arr[j] = tmp;
}

int do_partition(int pivot) {
    int i = 0, j = SIZE - 1;
    while (i < j) {
        while (i < j && arr[i] > pivot) i++;
        while (i < j && arr[j] <= pivot) j--;
        if (i < j) {
            swap_arr(i, j);
            i++;
            j--;
        }
    }
    return i;
}

void init_arr(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = (i * 17 + 23) % SIZE;
    }
}

long bench(void) {
    long sum = 0;
    for (int i = 0; i < ITERS; i++) {
        init_arr();
        sum += do_partition(i % SIZE);
    }
    return sum;
}

int main(void) {
    printf("%ld\n", bench());
    return 0;
}
