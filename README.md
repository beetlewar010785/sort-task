# 📂 Large File Sorter 🚀

## 📌 Overview

This project implements a **large file sorting system**, designed to efficiently sort files of **unlimited size** (tested on 1GB files). Each line in the file follows the format: **`Number. String`**.

The implementation leverages **B-Tree data structures** for optimal sorting performance.

The core class of the implementation is **BTreeIndexer<TOphValue>**, which performs indexing of the specified record.

---

## 🏗️ Architecture

The project follows the **Hexagonal Architecture** (Ports and Adapters) to ensure modularity and testability.

### 📂 Project Structure

- **src/SortTask.Adapter** - Adapter layer.
- **src/SortTask.Application** - Application layer.
- **src/SortTask.Domain** - Business logic layer.
- **src/SortTask.TestFileGenerator** - Generates a test unsorted file.
- **src/SortTask.Sorter** - Sorts the input file.
- **src/SortTask.Checker** - Verifies the sorted file.

---

## ⚙️ Installation

Ensure you have **.NET 8.0** installed before proceeding.

---

## 🎯 Running the Application

You can run the sorter using two approaches:

### 🖥️ Running from Source

```sh
FILE_SIZE={SIZE_IN_BYTES} make run-src
```

🔹 Runs directly from the source code.

### 🏗️ Running from Binaries

```sh
make publish && FILE_SIZE={SIZE_IN_BYTES} make run-bin
```

🔹 Runs the sorter from the compiled binaries.

### 🏗️ Running from Published Binary

```sh
dotnet publish -c Release -o ./bin && {FILE_SIZE} ./bin/SortTask.Sorter
```

🔹 Builds the binary and runs the sorter.

The sorted file will be saved in:

```
./bin/sorted
```

---

## 📜 License

This project is licensed under the **MIT License**.

🛠️ Happy Sorting! 🚀

