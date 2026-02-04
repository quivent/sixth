// expected: 1330170
// 128x128 matrix multiply, 10 times
// Checksum: sum of C[0][0] from each multiply

#include <stdio.h>

long A[128][128];
long B[128][128];
long C[128][128];

void init_mats(int iter) {
    for (int i = 0; i < 128; i++) {
        for (int j = 0; j < 128; j++) {
            A[i][j] = (i * 128 + j + iter) % 100;
            B[i][j] = (i * 128 + j + iter) % 50;
        }
    }
    for (int i = 0; i < 128; i++) {
        for (int j = 0; j < 128; j++) {
            C[i][j] = 0;
        }
    }
}

void matmul(void) {
    for (int i = 0; i < 128; i++) {
        for (int j = 0; j < 128; j++) {
            long sum = 0;
            for (int k = 0; k < 128; k++) {
                sum += A[i][k] * B[k][j];
            }
            C[i][j] = sum;
        }
    }
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 10; i++) {
        init_mats(i);
        matmul();
        sum += C[0][0];
    }
    printf("%ld\n", sum);
    return 0;
}
