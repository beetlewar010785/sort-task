using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Adapter;

// public class StreamBTreeReadWriter : IBTreeReadWriter<StreamBTreeIndex>
// {
//     public Task<BTreeNode<StreamBTreeIndex>?> ReadRoot()
//     {
//         throw new NotImplementedException();
//     }
//
//     public Task WriteRoot(BTreeNode<StreamBTreeIndex> root)
//     {
//         throw new NotImplementedException();
//     }
//
//     public Task<StreamBTreeIndex> WriteIndex(RowIndexKey key)
//     {
//         throw new NotImplementedException();
//     }
// }