// expected: 499500000
// Hash table linear probing benchmark - 1M lookups

#include <stdio.h>

#define TSIZE 2048
#define ITERS 1000000

int htbl[TSIZE];
int hval[TSIZE];

int hash(int key) {
    unsigned int h = (unsigned int)key;
    h = (h << 5) | (h >> 27);
    h ^= h;
    return h & (TSIZE - 1);
}

void hash_insert(int val, int key) {
    int h = hash(key);
    while (htbl[h] != -1) {
        h = (h + 1) & (TSIZE - 1);
    }
    htbl[h] = key;
    hval[h] = val;
}

int hash_find(int key) {
    int h = hash(key);
    while (htbl[h] != -1 && htbl[h] != key) {
        h = (h + 1) & (TSIZE - 1);
    }
    if (htbl[h] == -1) return -1;
    return hval[h];
}

void init_htbl(void) {
    for (int i = 0; i < TSIZE; i++) {
        htbl[i] = -1;
    }
    for (int i = 0; i < 1000; i++) {
        hash_insert(i, i);
    }
}

long bench(void) {
    long sum = 0;
    for (int i = 0; i < ITERS; i++) {
        sum += hash_find(i % 1000);
    }
    return sum;
}

int main(void) {
    init_htbl();
    printf("%ld\n", bench());
    return 0;
}
