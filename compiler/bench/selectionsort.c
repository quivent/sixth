// expected: 499500
// Selection sort benchmark - sort 1000 elements, sum result

#include <stdio.h>

#define SIZE 1000

int arr[SIZE];

void swap_arr(int i, int j) {
    int tmp = arr[i];
    arr[i] = arr[j];
    arr[j] = tmp;
}

int find_min(int start, int end) {
    int min_idx = start;
    for (int i = start + 1; i < end; i++) {
        if (arr[i] < arr[min_idx]) {
            min_idx = i;
        }
    }
    return min_idx;
}

void selection_sort(int n) {
    for (int i = 0; i < n; i++) {
        int min_idx = find_min(i, n);
        if (i != min_idx) {
            swap_arr(i, min_idx);
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
    selection_sort(SIZE);
    printf("%ld\n", sum_arr());
    return 0;
}
