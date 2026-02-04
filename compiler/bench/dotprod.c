// expected: 142100000000
// Dot product of 1000-element vectors, 100K times
// Checksum: sum of all dot products

#include <stdio.h>

long vecA[1000];
long vecB[1000];

void init_vecs(int iter) {
    for (int i = 0; i < 1000; i++) {
        vecA[i] = (i + iter) % 100;
        vecB[i] = (i + iter) % 50;
    }
}

long dot_product(void) {
    long sum = 0;
    for (int i = 0; i < 1000; i++) {
        sum += vecA[i] * vecB[i];
    }
    return sum;
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 100000; i++) {
        init_vecs(i);
        sum += dot_product();
    }
    printf("%ld\n", sum);
    return 0;
}
