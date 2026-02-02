// expected: 1254400
#include <stdio.h>
#define N 20
long A[N][N], B[N][N], C[N][N];
int main() {
    for (int i = 0; i < N; i++)
        for (int j = 0; j < N; j++) {
            A[i][j] = i + j;
            B[i][j] = j - i;
            C[i][j] = 0;
        }
    for (int i = 0; i < N; i++)
        for (int j = 0; j < N; j++)
            for (int k = 0; k < N; k++)
                C[i][j] += A[i][k] * B[k][j];
    long sum = 0;
    for (int i = 0; i < N; i++)
        for (int j = 0; j < N; j++)
            sum += C[i][j];
    printf("%ld\n", sum);
    return 0;
}
