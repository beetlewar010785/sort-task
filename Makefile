GENERATOR_BIN = $(BUILD_DIR)/generator
SORTER_BIN = $(BUILD_DIR)/sorter
CHECKER_BIN = $(BUILD_DIR)/checker

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
ifndef BTREE_ORDER
	$(error "ERROR: BTREE_ORDER is not set")
endif

build: check-build-env
	mkdir -p $(BUILD_DIR)
	dotnet publish ./src/SortTask.TestFileCreator -c Release -o $(GENERATOR_BIN)
	dotnet publish ./src/SortTask.Sorter -c Release -o $(SORTER_BIN)
	dotnet publish ./src/SortTask.Checker -c Release -o $(CHECKER_BIN)

run: check-run-env generate sort check

run-bin: build check-run-env
	$(GENERATOR_BIN)/SortTask.TestFileCreator -f $(UNSORTED_FILE) -s $(FILE_SIZE)
	$(SORTER_BIN)/SortTask.Sorter -u $(UNSORTED_FILE) -x $(INDEX_FILE) -s $(SORTED_FILE) -o $(BTREE_ORDER)
	$(CHECKER_BIN)/SortTask.Checker -f $(SORTED_FILE)

generate:
	dotnet run --project ./src/SortTask.TestFileCreator -- -f $(UNSORTED_FILE) -s $(FILE_SIZE)

sort:
	dotnet run --project ./src/SortTask.Sorter -- -u $(UNSORTED_FILE) -x $(INDEX_FILE) -s $(SORTED_FILE) -o $(BTREE_ORDER)

check:
	dotnet run --project ./src/SortTask.Checker -- -f $(SORTED_FILE)
