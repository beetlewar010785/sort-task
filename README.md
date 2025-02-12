UNSORTED_FILE=output/unsorted INDEX_FILE=output/index SORTED_FILE=output/sorted FILE_SIZE=100000000 make run-src
BUILD_DIR=output/bin make publish
UNSORTED_FILE=output/unsorted INDEX_FILE=output/index SORTED_FILE=output/sorted FILE_SIZE=100000000 BUILD_DIR=output/bin make run-bin

FILE_SIZE=10737418
00:00:31.9309121

FILE_SIZE=1073741899
00:32:27.8623078

FILE_SIZE=10737418
00:00:10.6093132

FILE_SIZE=1073741899
00:34:55.8623078

dotnet add package StyleCop.Analyzers
dotnet add package Microsoft.CodeAnalysis.FxCopAnalyzers
dotnet tool install -g dotnet-format