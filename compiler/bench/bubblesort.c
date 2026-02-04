// expected: 499500
// Bubble sort benchmark - sort 1000 elements descending, sum result

#include <stdio.h>

#define SIZE 1000

int arr[SIZE];

void swap_arr(int i, int j) {
    int tmp = arr[i];
    arr[i] = arr[j];
    arr[j] = tmp;
}

void bubble_sort(int n) {
    for (int i = 0; i < n; i++) {
        for (int j = 0; j < n - 1 - i; j++) {
            if (arr[j] > arr[j + 1]) {
                swap_arr(j, j + 1);
            }
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
    bubble_sort(SIZE);
    printf("%ld\n", sum_arr());
    return 0;
}
