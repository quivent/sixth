// expected: 49995000
// Bucket sort benchmark - sort 10000 elements

#include <stdio.h>

#define SIZE 10000
#define NBUCKETS 100

int arr[SIZE];
int buckets[NBUCKETS][SIZE];
int bcount[NBUCKETS];

int bucket_idx(int val) {
    return val / SIZE;
}

void insert_bucket(int val) {
    int b = bucket_idx(val);
    buckets[b][bcount[b]++] = val;
}

void sort_bucket(int b) {
    // Insertion sort on bucket b
    for (int i = 1; i < bcount[b]; i++) {
        int key = buckets[b][i];
        int j = i;
        while (j > 0 && buckets[b][j - 1] < key) {
            buckets[b][j] = buckets[b][j - 1];
            j--;
        }
        buckets[b][j] = key;
    }
}

void collect_buckets(void) {
    int idx = 0;
    // Descending order
    for (int b = NBUCKETS - 1; b >= 0; b--) {
        for (int i = 0; i < bcount[b]; i++) {
            arr[idx++] = buckets[b][i];
        }
    }
}

void bucket_sort(void) {
    for (int i = 0; i < NBUCKETS; i++) bcount[i] = 0;
    for (int i = 0; i < SIZE; i++) insert_bucket(arr[i]);
    for (int i = 0; i < NBUCKETS; i++) sort_bucket(i);
    collect_buckets();
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
    bucket_sort();
    printf("%ld\n", sum_arr());
    return 0;
}
