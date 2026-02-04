// expected: 499500
// Reverse array benchmark - 10K iterations

#include <stdio.h>

#define SIZE 1000
#define ITERS 10000

int arr[SIZE];

void swap_arr(int i, int j) {
    int tmp = arr[i];
    arr[i] = arr[j];
    arr[j] = tmp;
}

void reverse_arr(void) {
    int i = 0, j = SIZE - 1;
    while (i < j) {
        swap_arr(i, j);
        i++;
        j--;
    }
}

void init_arr(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = i;
    }
}

long sum_arr(void) {
    long sum = 0;
    for (int i = 0; i < SIZE; i++) {
        sum += arr[i];
    }
    return sum;
}

long bench(void) {
    init_arr();
    for (int i = 0; i < ITERS; i++) {
        reverse_arr();
    }
    return sum_arr();
}

int main(void) {
    printf("%ld\n", bench());
    return 0;
}
