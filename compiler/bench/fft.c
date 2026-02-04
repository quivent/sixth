// expected: 511500000
// FFT on 1024 elements, 1000 times (integer approximation)
// Uses fixed-point arithmetic scaled by 1000
// Checksum: sum of first element of result * iterations

#include <stdio.h>

long real[1024];
long imag[1024];
long tmp_r[1024];
long tmp_i[1024];

#define SCALE 1000

void init_data(int iter) {
    for (int i = 0; i < 1024; i++) {
        real[i] = ((i + iter) % 1024) * SCALE;
        imag[i] = 0;
    }
}

void fft_iter(int size) {
    int half = size / 2;
    for (int i = 0; i < half; i++) {
        for (int j = 0; j < 1024 / size; j++) {
            int idx = j * size + i;
            int idx2 = idx + half;
            long r1 = real[idx];
            long r2 = real[idx2];
            tmp_r[idx] = (r1 + r2) / 2;
            tmp_r[idx2] = (r1 - r2) / 2;
            long i1 = imag[idx];
            long i2 = imag[idx2];
            tmp_i[idx] = (i1 + i2) / 2;
            tmp_i[idx2] = (i1 - i2) / 2;
        }
    }
    for (int i = 0; i < 1024; i++) {
        real[i] = tmp_r[i];
        imag[i] = tmp_i[i];
    }
}

void simple_fft(void) {
    for (int size = 2; size <= 1024; size *= 2) {
        fft_iter(size);
    }
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 1000; i++) {
        init_data(i);
        simple_fft();
        sum += real[0];
    }
    printf("%ld\n", sum);
    return 0;
}
