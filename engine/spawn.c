/* spawn.c - Native concurrency for Sixth
 *
 * Adds spawn/wait primitives using pthreads.
 * Each spawned word runs in its own thread with its own VM.
 * Thread state is per-VM (no global mutable state).
 */

#include "fifth.h"
#include <pthread.h>
#include <unistd.h>

#define MAX_THREADS 64

typedef struct {
    pthread_t thread;
    vm_t *vm;           /* Each thread gets its own VM */
    int xt;             /* Word to execute */
    bool active;
    bool done;
    cell_t result;      /* TOS after execution */
} thread_slot_t;

typedef struct {
    thread_slot_t slots[MAX_THREADS];
    pthread_mutex_t mutex;
    int next_id;
} thread_state_t;

/* Get per-VM thread state, lazily allocate */
static thread_state_t *get_state(vm_t *vm) {
    if (!vm->thread_slots) {
        thread_state_t *ts = calloc(1, sizeof(thread_state_t));
        pthread_mutex_init(&ts->mutex, NULL);
        ts->next_id = 0;
        vm->thread_slots = ts;
    }
    return (thread_state_t *)vm->thread_slots;
}

/* Thread entry point */
static void *thread_runner(void *arg) {
    thread_slot_t *slot = (thread_slot_t *)arg;

    /* Execute the word */
    vm_execute(slot->vm, slot->xt);

    /* Capture result (TOS) */
    if (depth(slot->vm) > 0) {
        slot->result = pop(slot->vm);
    } else {
        slot->result = 0;
    }

    slot->done = true;
    return NULL;
}

/* Clone a VM for a new thread */
static vm_t *vm_clone(vm_t *parent) {
    vm_t *child = calloc(1, sizeof(vm_t));
    if (!child) return NULL;

    /* Copy dictionary and memory */
    memcpy(child->dict, parent->dict, sizeof(parent->dict));
    child->dict_count = parent->dict_count;
    child->latest = parent->latest;
    memcpy(child->mem, parent->mem, parent->here);
    child->here = parent->here;

    /* Fresh stacks */
    child->sp = child->dstack + DSTACK_SIZE;
    child->rsp = child->rstack + RSTACK_SIZE;

    /* Copy state */
    child->base = parent->base;
    child->running = true;
    child->out = parent->out;

    /* Copy cached XTs */
    child->xt_lit = parent->xt_lit;
    child->xt_branch = parent->xt_branch;
    child->xt_0branch = parent->xt_0branch;
    child->xt_exit = parent->xt_exit;
    child->xt_slit = parent->xt_slit;
    child->xt_do = parent->xt_do;
    child->xt_qdo = parent->xt_qdo;
    child->xt_loop = parent->xt_loop;
    child->xt_ploop = parent->xt_ploop;
    child->xt_does = parent->xt_does;

    /* Child gets its own thread state (NULL = lazy init) */
    child->thread_slots = NULL;

    return child;
}

/* SPAWN ( xt -- thread-id ) */
static void p_spawn(vm_t *vm) {
    int xt = (int)pop(vm);
    thread_state_t *ts = get_state(vm);

    pthread_mutex_lock(&ts->mutex);

    /* Find free slot */
    int id = -1;
    for (int i = 0; i < MAX_THREADS; i++) {
        int idx = (ts->next_id + i) % MAX_THREADS;
        if (!ts->slots[idx].active) {
            id = idx;
            ts->next_id = (idx + 1) % MAX_THREADS;
            break;
        }
    }

    if (id < 0) {
        pthread_mutex_unlock(&ts->mutex);
        fprintf(stderr, "SPAWN: No free thread slots\n");
        push(vm, -1);
        return;
    }

    /* Set up thread */
    ts->slots[id].vm = vm_clone(vm);
    ts->slots[id].xt = xt;
    ts->slots[id].active = true;
    ts->slots[id].done = false;
    ts->slots[id].result = 0;

    /* Create thread */
    if (pthread_create(&ts->slots[id].thread, NULL, thread_runner, &ts->slots[id]) != 0) {
        free(ts->slots[id].vm);
        ts->slots[id].active = false;
        pthread_mutex_unlock(&ts->mutex);
        fprintf(stderr, "SPAWN: pthread_create failed\n");
        push(vm, -1);
        return;
    }

    pthread_mutex_unlock(&ts->mutex);
    push(vm, id);
}

/* WAIT ( thread-id -- result ) */
static void p_wait(vm_t *vm) {
    int id = (int)pop(vm);
    thread_state_t *ts = get_state(vm);

    if (id < 0 || id >= MAX_THREADS || !ts->slots[id].active) {
        fprintf(stderr, "WAIT: Invalid thread ID %d\n", id);
        push(vm, 0);
        return;
    }

    pthread_join(ts->slots[id].thread, NULL);

    cell_t result = ts->slots[id].result;

    /* Cleanup */
    free(ts->slots[id].vm);
    ts->slots[id].active = false;

    push(vm, result);
}

/* WAIT-ALL ( -- ) */
static void p_wait_all(vm_t *vm) {
    thread_state_t *ts = get_state(vm);

    for (int i = 0; i < MAX_THREADS; i++) {
        if (ts->slots[i].active) {
            pthread_join(ts->slots[i].thread, NULL);
            free(ts->slots[i].vm);
            ts->slots[i].active = false;
        }
    }
}

/* THREAD-DONE? ( thread-id -- flag ) */
static void p_thread_done(vm_t *vm) {
    int id = (int)pop(vm);
    thread_state_t *ts = get_state(vm);

    if (id < 0 || id >= MAX_THREADS || !ts->slots[id].active) {
        push(vm, -1);  /* Invalid = done */
        return;
    }

    push(vm, ts->slots[id].done ? -1 : 0);
}

/* NPROC ( -- n ) */
static void p_nproc(vm_t *vm) {
#ifdef _SC_NPROCESSORS_ONLN
    long n = sysconf(_SC_NPROCESSORS_ONLN);
    push(vm, n > 0 ? n : 1);
#else
    push(vm, 1);
#endif
}

/* Initialize spawn primitives */
void spawn_init(vm_t *vm) {
    vm_add_prim(vm, "spawn", p_spawn, false);
    vm_add_prim(vm, "wait", p_wait, false);
    vm_add_prim(vm, "wait-all", p_wait_all, false);
    vm_add_prim(vm, "thread-done?", p_thread_done, false);
    vm_add_prim(vm, "nproc", p_nproc, false);
}
