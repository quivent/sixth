// expected: 7771200
// 64x64 matrix multiply, 100 times
// Checksum: sum of C[0][0] from each multiply

#include <stdio.h>

long A[64][64];
long B[64][64];
long C[64][64];

void init_mats(int iter) {
    for (int i = 0; i < 64; i++) {
        for (int j = 0; j < 64; j++) {
            A[i][j] = (i * 64 + j + iter) % 100;
            B[i][j] = (i * 64 + j + iter) % 50;
        }
    }
    for (int i = 0; i < 64; i++) {
        for (int j = 0; j < 64; j++) {
            C[i][j] = 0;
        }
    }
}

void matmul(void) {
    for (int i = 0; i < 64; i++) {
        for (int j = 0; j < 64; j++) {
            long sum = 0;
            for (int k = 0; k < 64; k++) {
                sum += A[i][k] * B[k][j];
            }
            C[i][j] = sum;
        }
    }
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 100; i++) {
        init_mats(i);
        matmul();
        sum += C[0][0];
    }
    printf("%ld\n", sum);
    return 0;
}
