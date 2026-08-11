<div align="center">

```text
  ___ _____  _ _____ _  _ 
 / __|_ _\ \/ |_   _| || |
 \__ \| | >  <  | | | __ |
 |___/___/_/\_\ |_| |_||_|
```

**A Forth for the Agentic Era**

*One binary, zero dependencies, instant startup.*

[![Language](https://img.shields.io/badge/Language-C%20%7C%20Forth-blue.svg?style=for-the-badge)](#)
[![Platform](https://img.shields.io/badge/Platform-macOS%20%7C%20Linux%20%7C%20Windows-lightgrey.svg?style=for-the-badge)](#)
[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](#)

</div>

---

> *"I think the industry is fundamentally unable to appreciate simplicity."*
> — Chuck Moore, creator of Forth

## ⚡ Overview

Sixth is a self-contained Forth ecosystem designed for AI-assisted development. Built on [Fifth](https://github.com/quivent/fifth), its predecessor, Sixth brings a one binary, zero dependencies, instant startup philosophy. 

The explicit stack model and small vocabulary make it uniquely suited for LLM code generation — where other languages struggle with implicit state and sprawling APIs, Forth's simplicity becomes an advantage. Write tools that parse data, generate HTML, query databases — and optionally compile them to native code when you need speed.

---

## ✨ Features

- **Built for AI Coding**: Explicit state eliminates hallucination vectors. The small vocabulary (~75 words) is easily retained in LLM context.
- **Native I/O**: Skip the shell! `open-path` calls macOS `LSOpenCFURLRef` directly from C for zero subprocess overhead (48ms execution).
- **Lightweight & Fast**: A 57KB standalone interpreter binary featuring <1ms startup time.
- **Flexible Backends**: Use the C interpreter for scripts, or compile to native ARM64/x86_64 binaries via Cranelift for production speeds (70-85% of C).

---

## 📦 Installation

### Homebrew (macOS)
```bash
brew tap quivent/sixth
brew install sixth
```

### From Source (30 seconds)
```bash
git clone https://github.com/quivent/sixth.git
cd sixth && cd engine && make && cd ..
./engine/fifth install.fs
```
> [!NOTE]
> Sixth installs itself to `/usr/local/bin`. Then just `sixth` from anywhere.

<details>
<summary>Alternative: Manual install</summary>

```bash
git clone https://github.com/quivent/sixth.git
cd sixth
cd engine && make && cd ..
mkdir -p ~/.sixth/lib ~/.sixth/packages
cp -r lib/* ~/.sixth/lib/
sudo cp engine/sixth /usr/local/bin/
sixth -e "2 3 + . cr"   # Should print: 5
```
</details>

---

## 🚀 Usage

### Hello, World
```bash
sixth -e ': hello ." Hello, World!" cr ; hello'
```

### Build a Dashboard
```forth
require ~/.sixth/lib/pkg.fs
use lib:core.fs
use lib:ui.fs

s" /tmp/dashboard.html" w/o create-file throw html>file
s" System Status" html-head ui-css html-body

grid-auto-begin
  42 s" Users" stat-card-n
  7 s" Active" stat-card-n
  99 s" Uptime %" stat-card-n
grid-end

html-end
html-fid @ close-file throw
```

### Package System
Sixth uses `~/.sixth/` as its package home (configurable via `SIXTH_HOME`).
```forth
\ Bootstrap the package system first
require ~/.sixth/lib/pkg.fs

\ Load core libraries
use lib:str.fs           \ String buffers
use lib:sql.fs           \ SQLite interface
use lib:core.fs          \ Loads all core libs

\ Load a package
use pkg:my-package
```

---

## 📖 Architecture & Benchmarks

```text
              YOUR FORTH CODE
              : square dup * ;
                    │
      ┌─────────────┼─────────────┐
      ▼             ▼             ▼
 ./engine/fifth   ./engine/fifth   ./engine/fifth
(default)       compile       --emit-c
      │             │             │
      ▼             ▼             ▼
 C Interpreter  Cranelift     gcc/clang
 <1ms startup   JIT/AOT       native
 5-15% of C     70-85% of C   50-70% of C
```

| Backend | Startup | Speed vs C | Binary Size | Use Case |
|---------|---------|------------|-------------|----------|
| **Interpreter** | <1ms | 5-15% | 57 KB | Development, scripts, CLI tools |
| **Cranelift JIT** | ~50ms | 70-85% | 10-50 KB | Production binaries |
| **C Codegen** | 2-20ms | 40-70% | 10-50 KB | Embedding, portability |

> [!TIP]
> See [docs/agentic-coding.md](docs/agentic-coding.md) for a deep dive into why LLMs generate better Forth than Python.

---

## 🤝 Contributing

Sixth grows by solving real problems. If you build something useful, extract the reusable words and submit them. See [docs/contributing.md](docs/contributing.md).

---

## 📄 License

MIT

> *"Simplicity is prerequisite for reliability."* — Edsger Dijkstra
