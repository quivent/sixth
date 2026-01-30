#include <stdio.h>
#include <stdint.h>

int main() {
    uint64_t sum = 0;
    for (uint64_t i = 1; i <= 1000000000ULL; i++) {
        sum += i;
    }
    printf("%lu\n", sum);
    return 0;
}
