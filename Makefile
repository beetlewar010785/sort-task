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

run: generate sort check

generate:
	dotnet run --project ./src/SortTask.TestFileCreator -- -f $(UNSORTED_FILE) -s $(FILE_SIZE)

sort:
	dotnet run --project ./src/SortTask.Sorter -- -u $(UNSORTED_FILE) -x $(INDEX_FILE) -s $(SORTED_FILE) -o ${BTREE_ORDER}

check:
	dotnet run --project ./src/SortTask.Checker -- -f $(SORTED_FILE)
