// expected: 49995000
// Shell sort benchmark - sort 10000 elements, sum result

#include <stdio.h>

#define SIZE 10000

int arr[SIZE];

void shell_sort(void) {
    for (int gap = SIZE / 2; gap > 0; gap /= 2) {
        for (int i = gap; i < SIZE; i++) {
            int key = arr[i];
            int j = i;
            while (j >= gap && arr[j - gap] < key) {
                arr[j] = arr[j - gap];
                j -= gap;
            }
            arr[j] = key;
        }
    }
}

void init_arr(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = SIZE - i - 1;
    }
}

long sum_arr(void) {
    long sum = 0;
    for (int i = 0; i < SIZE; i++) {
        sum += arr[i];
    }
    return sum;
}

int main(void) {
    init_arr();
    shell_sort();
    printf("%ld\n", sum_arr());
    return 0;
}
