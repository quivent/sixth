#include <stdio.h>
#include <stdint.h>

int main() {
    for (volatile uint64_t i = 10000000000ULL; i > 0; i--);
    printf("done\n");
    return 0;
}
