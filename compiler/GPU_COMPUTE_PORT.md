# GPU Compute Shader Port Specification

## Overview

Compile Forth to GPU compute shaders for massively parallel execution. This is architecturally different from CPU targets — the execution model changes fundamentally.

## Target Options

| Target | Platform | Notes |
|--------|----------|-------|
| Metal Compute | macOS/iOS | Native Apple, best M4 Max performance |
| SPIR-V | Vulkan (cross-platform) | Works on Mac via MoltenVK |
| WGSL | WebGPU | Emerging standard, browser + native |
| CUDA | NVIDIA only | Not relevant for Mac |

**Recommendation:** Metal Compute for M4 Max (40 GPU cores, 546 GB/s bandwidth)

## Fundamental Differences

### CPU (current sixth)
```
- Single thread of execution
- Deep call stack, recursion OK
- Arbitrary control flow
- Stack in memory, cheap access
- Sequential by default
```

### GPU Compute
```
- Thousands of threads execute same code
- No recursion (or very limited)
- Divergent branches are expensive
- Registers precious, memory access expensive
- Parallel by default
```

## Execution Model Change

### CPU Forth
```forth
: sum-array ( addr n -- sum )
  0 swap 0 do
    over i cells + @ +
  loop nip ;
```
One thread, loops through array sequentially.

### GPU Forth
```forth
: parallel-sum ( addr n -- sum )
  \ Each GPU thread handles one element
  thread-id cells + @      \ Load my element
  workgroup-reduce-add ;   \ Hardware-assisted reduction
```
Thousands of threads, each handles one element, reduction combines results.

## Two Compilation Modes

### Mode 1: Whole-Program GPU (data parallelism)
- Entire Forth program runs on GPU
- Stack per thread (in registers or local memory)
- Best for: array processing, simulations, neural nets

### Mode 2: GPU Kernels from CPU (hybrid)
- CPU runs main program
- Specific words compile to GPU kernels
- `gpu{` ... `}gpu` syntax marks parallel sections
- Best for: mixed workloads, easier migration

## Stack Implementation on GPU

### Option A: Register Stack (fastest, limited depth)
```
s0 = TOS
s1 = NOS
s2 = 3rd
s3 = 4th
```
Metal has 32 registers per thread. Stack depth ~8-12 practical.

### Option B: Thread-Local Memory Stack
```metal
threadgroup float stack[THREADS_PER_GROUP][MAX_DEPTH];
uint sp = MAX_DEPTH;  // grows down
```
Slower but unlimited depth. Shared memory is ~100x slower than registers.

### Option C: Hybrid
Registers for top 4, spill to local memory.

**Recommendation:** Option C — mirrors current sixth register strategy.

## Word Classification

### Trivially Parallel (map directly)
```forth
+  -  *  /           \ arithmetic
and or xor invert    \ bitwise
@ !                  \ memory (with care)
dup drop swap        \ stack ops
```

### Requires Reduction (need GPU primitives)
```forth
sum    \ parallel reduction
min    \ parallel reduction
max    \ parallel reduction
any    \ parallel OR reduction
all    \ parallel AND reduction
```

### Problematic (need restructuring)
```forth
.       \ I/O — GPU can't do console output
emit    \ I/O
key     \ I/O
recurse \ recursion — limited or forbidden
begin/until with data-dependent exit  \ divergence
```

### Forbidden (must stay on CPU)
```forth
include require      \ file operations
open-file close-file \ file I/O
allocate free        \ dynamic allocation
```

## New Primitives for GPU

```forth
\ Thread identification
thread-id        ( -- n )        \ global thread index
local-id         ( -- n )        \ index within workgroup
workgroup-id     ( -- n )        \ which workgroup
num-threads      ( -- n )        \ total threads launched

\ Synchronization
barrier          ( -- )          \ sync all threads in workgroup

\ Reductions (hardware-accelerated)
workgroup-sum    ( n -- sum )    \ sum across workgroup
workgroup-min    ( n -- min )
workgroup-max    ( n -- max )

\ Atomic operations
atomic-add       ( n addr -- )   \ atomic memory add
atomic-cas       ( old new addr -- actual )  \ compare-and-swap

\ Memory
global@          ( addr -- n )   \ read global memory
global!          ( n addr -- )   \ write global memory
local@           ( addr -- n )   \ read workgroup shared memory
local!           ( n addr -- )   \ write workgroup shared memory
```

## Metal Shading Language Mapping

### Forth Stack Ops → Metal
```metal
// gen-dup
float tos_copy = tos;
stack[--sp] = nos;
nos = tos_copy;

// gen-add
tos = nos + tos;
nos = stack[sp++];

// gen-lit (push constant)
stack[--sp] = nos;
nos = tos;
tos = 42.0;  // the literal
```

### Forth Control Flow → Metal
```forth
\ Forth
x 0> if positive else negative then

// Metal (both branches execute, result selected)
float pos_result = /* positive branch */;
float neg_result = /* negative branch */;
tos = (x > 0) ? pos_result : neg_result;
```

GPU executes both branches, masks results. Divergence is expensive but not forbidden.

## Compilation Pipeline

```
sixth source
    │
    ▼
┌─────────────────┐
│ Parse + Analyze │  (existing two-pass)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Classify Words  │  parallel / reduction / sequential
└────────┬────────┘
         │
    ┌────┴────┐
    ▼         ▼
┌───────┐ ┌───────┐
│ CPU   │ │ GPU   │
│ Code  │ │ Kernel│
└───────┘ └───────┘
    │         │
    ▼         ▼
  ELF/     Metal IR
  Mach-O   or SPIR-V
```

## Memory Model

### CPU sixth
```
CODE-BASE  = 0x400000   (code segment)
DATA-BASE  = 0x800000   (variables)
Stack in registers + memory
```

### GPU sixth
```
Global Memory   - large, slow (~400 cycles)
Shared Memory   - per workgroup, fast (~20 cycles)
Registers       - per thread, fastest (1 cycle)
Constants       - broadcast to all threads
```

Strategy:
- Input arrays: Global memory (read-only if possible)
- Output arrays: Global memory
- Stack top 4: Registers
- Stack overflow: Shared memory
- Loop counters: Registers

## Example: Vector Addition

### CPU sixth
```forth
: vadd ( a b c n -- )  \ c = a + b
  0 do
    over i cells + @     \ a[i]
    2 pick i cells + @   \ b[i]
    +
    2 pick i cells + !   \ c[i] = sum
  loop
  drop drop drop ;
```

### GPU sixth
```forth
: vadd ( a b c -- )  \ launched with n threads
  thread-id cells    ( a b c offset )
  >r                 ( a b c ) ( R: offset )
  rot r@ + @         ( b c a[i] )
  rot r@ + @         ( c a[i] b[i] )
  +                  ( c sum )
  swap r> + ! ;      ( ) store c[i]
```

Launched as: `a b c n gpu-launch vadd`

## Implementation Phases

### Phase 1: Metal IR Emitter
- Emit Metal Shading Language text (not binary)
- Let Metal compiler optimize
- Easier debugging

### Phase 2: Basic Parallel Words
- Arithmetic, stack ops, memory access
- Single kernel, all threads do same thing

### Phase 3: Reductions
- Implement workgroup-sum, min, max
- Two-pass reduction for large arrays

### Phase 4: Control Flow
- Flatten if/then/else to select()
- Convert simple loops to thread mapping

### Phase 5: CPU/GPU Hybrid
- Mark parallel sections
- Auto-generate dispatch code
- Handle data transfer

## Limitations

| Feature | GPU Support |
|---------|-------------|
| Recursion | No (or very limited depth) |
| Dynamic allocation | No |
| File I/O | No (CPU only) |
| Console I/O | No (CPU only) |
| Variable loop bounds per thread | Expensive (divergence) |
| Indirect jumps | No |
| Deep stacks | Limited (~32 elements practical) |

## Performance Expectations

For parallel workloads on M4 Max (40 GPU cores, 546 GB/s):

| Workload | CPU (16 cores) | GPU (40 cores) | Speedup |
|----------|----------------|----------------|---------|
| Vector add (1M elements) | ~2ms | ~0.05ms | 40x |
| Matrix multiply (1K x 1K) | ~100ms | ~2ms | 50x |
| Reduction (sum 1M) | ~1ms | ~0.1ms | 10x |
| Sequential code | baseline | N/A | - |

GPU wins big on data parallelism, loses on sequential or divergent code.

## Effort Estimate

| Task | Effort |
|------|--------|
| Metal IR text emitter | 2-3 days |
| Basic parallel gen-* | 2-3 days |
| Reduction primitives | 2-3 days |
| Control flow flattening | 3-5 days |
| CPU/GPU dispatch | 3-5 days |
| Testing + optimization | 5-10 days |
| **Total** | **3-4 weeks** |

Significantly more than ARM64 port due to execution model change.

## Alternative: Keep CPU, Use GPU for Specific Words

Instead of full GPU compilation, add GPU-accelerated primitives:

```forth
\ CPU code calls GPU for heavy lifting
1000000 floats allocate constant data
data 1000000 gpu-fill-random   \ GPU fills array
data 1000000 gpu-sum .         \ GPU reduction
data 1000000 gpu-sort          \ GPU sort
```

This is less ambitious but immediately useful. Could be Phase 0.

## References

- [Metal Shading Language Spec](https://developer.apple.com/metal/Metal-Shading-Language-Specification.pdf)
- [Metal Best Practices](https://developer.apple.com/documentation/metal/gpu_programming_guide)
- [GPU Parallel Reduction](https://developer.download.nvidia.com/assets/cuda/files/reduction.pdf)
- [SPIR-V Spec](https://www.khronos.org/registry/SPIR-V/specs/unified1/SPIRV.html)
