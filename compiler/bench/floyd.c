// expected: 90500
#include <stdio.h>
#define N 100
#define INF 1000000
long dist[N][N];

void init() {
    for (int i = 0; i < N; i++)
        for (int j = 0; j < N; j++)
            dist[i][j] = (i == j) ? 0 : INF;
    for (int i = 1; i < N; i++) {
        dist[i][i-1] = 1;
        dist[i-1][i] = 1;
    }
    for (int i = 10; i < N; i++) {
        dist[i][i-10] = 2;
        dist[i-10][i] = 2;
    }
}

void floyd() {
    for (int k = 0; k < N; k++)
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++) {
                long alt = dist[i][k] + dist[k][j];
                if (alt < dist[i][j])
                    dist[i][j] = alt;
            }
}

int main() {
    init();
    floyd();
    long sum = 0;
    for (int i = 0; i < N; i++)
        for (int j = 0; j < N; j++)
            if (dist[i][j] < INF)
                sum += dist[i][j];
    printf("%ld\n", sum);
    return 0;
}
