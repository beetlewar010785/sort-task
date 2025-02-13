GENERATOR_BIN = bin/generator
SORTER_BIN = bin/sorter
CHECKER_BIN = bin/checker

UNSORTED_FILE = bin/unsorted
SORTED_FILE = bin/sorted
INDEX_FILE = bin/index

lint:
	dotnet format --verify-no-changes
	jb inspectcode SortTask.sln --output=bin/inspect-results.sarif

lint-fix:
	dotnet format
	jb cleanupcode SortTask.sln

test:
	dotnet test --logger:"console;verbosity=normal" --no-restore --no-build

check-run-env:
ifndef FILE_SIZE
	$(error "ERROR: FILE_SIZE is not set")
endif

publish:
	mkdir -p bin
	dotnet publish -c Release -r linux-x64 --self-contained false -o $(GENERATOR_BIN) ./src/SortTask.TestFileCreator
	dotnet publish -c Release -r linux-x64 --self-contained false -o $(SORTER_BIN) ./src/SortTask.Sorter
	dotnet publish -c Release -r linux-x64 --self-contained false -o $(CHECKER_BIN) ./src/SortTask.Checker

run-bin: check-run-env generate-bin sort-bin check-bin

generate-bin:
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

clean:
	dotnet clean
	rm -rf bin obj