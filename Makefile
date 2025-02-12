GENERATOR_BIN = $(BUILD_DIR)/generator
SORTER_BIN = $(BUILD_DIR)/sorter
CHECKER_BIN = $(BUILD_DIR)/checker

lint:
	dotnet format --verify-no-changes

lint-fix:
	dotnet format

check-build-env:
ifndef BUILD_DIR
	$(error "ERROR: BUILD_DIR is not set")
endif

check-run-env:
ifndef UNSORTED_FILE
	$(error "ERROR: UNSORTED_FILE is not set")
endif
ifndef SORTED_FILE
	$(error "ERROR: SORTED_FILE is not set")
endif
ifndef INDEX_FILE
	$(error "ERROR: INDEX_FILE is not set")
endif
ifndef FILE_SIZE
	$(error "ERROR: FILE_SIZE is not set")
endif

build: check-build-env
	mkdir -p $(BUILD_DIR)
	dotnet publish -c Release -r linux-x64 --self-contained false -o $(GENERATOR_BIN) ./src/SortTask.TestFileCreator
	dotnet publish -c Release -r linux-x64 --self-contained false -o $(SORTER_BIN) ./src/SortTask.Sorter
	dotnet publish -c Release -r linux-x64 --self-contained false -o $(CHECKER_BIN) ./src/SortTask.Checker

run-bin: check-run-env generate-bin sort-bin check-bin

generate-bin: check-build-env
	dotnet $(GENERATOR_BIN)/SortTask.TestFileCreator.dll -f $(UNSORTED_FILE) -s $(FILE_SIZE)

sort-bin:
	dotnet $(SORTER_BIN)/SortTask.Sorter.dll -u $(UNSORTED_FILE) -x $(INDEX_FILE) -s $(SORTED_FILE)

check-bin:
	dotnet $(CHECKER_BIN)/SortTask.Checker.dll -f $(SORTED_FILE)

run-src: check-run-env generate-src sort-src check-src

generate-src:
	dotnet run --project ./src/SortTask.TestFileCreator -- -f $(UNSORTED_FILE) -s $(FILE_SIZE)

sort-src:
	dotnet run --project ./src/SortTask.Sorter -- -u $(UNSORTED_FILE) -x $(INDEX_FILE) -s $(SORTED_FILE)

check-src:
	dotnet run --project ./src/SortTask.Checker -- -f $(SORTED_FILE)
