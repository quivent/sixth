// expected: 50000
// Anagram detection benchmark - check anagram pairs, run 50000 times

#include <stdio.h>
#include <string.h>

char word1[32];
char word2[32];
int counts1[26];
int counts2[26];

void init_words(void) {
    strcpy(word1, "listen");
    strcpy(word2, "silent");
}

void clear_counts(void) {
    memset(counts1, 0, sizeof(counts1));
    memset(counts2, 0, sizeof(counts2));
}

void count_word(const char *s, int len, int *counts) {
    for (int i = 0; i < len; i++) {
        counts[s[i] - 'a']++;
    }
}

int compare_counts(void) {
    for (int i = 0; i < 26; i++) {
        if (counts1[i] != counts2[i]) return 0;
    }
    return 1;
}

int anagram(void) {
    clear_counts();
    count_word(word1, 6, counts1);
    count_word(word2, 6, counts2);
    return compare_counts();
}

int main(void) {
    init_words();
    int result = 0;
    for (int i = 0; i < 50000; i++) {
        if (anagram()) result++;
    }
    printf("%d\n", result);
    return 0;
}
