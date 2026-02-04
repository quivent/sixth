// expected: 12497500
// Insertion sort benchmark - sort 5000 elements, sum result

#include <stdio.h>

#define SIZE 5000

int arr[SIZE];

void isort(void) {
    for (int i = 1; i < SIZE; i++) {
        int key = arr[i];
        int j = i;
        while (j > 0 && arr[j - 1] < key) {
            arr[j] = arr[j - 1];
            j--;
        }
        arr[j] = key;
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
    isort();
    printf("%ld\n", sum_arr());
    return 0;
}
