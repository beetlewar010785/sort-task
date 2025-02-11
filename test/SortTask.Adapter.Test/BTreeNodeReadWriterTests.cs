// using Newtonsoft.Json.Linq;
// using SortTask.Adapter.StreamBTree;
// using SortTask.Domain.BTree;
//
// namespace SortTask.Adapter.Test;
//
// public class BTreeNodeReadWriterTests
// {
//     [TestCaseSource(nameof(Cases))]
//     public async Task Should_Read_Written_Node(StreamBTreeNode node, BTreeOrder order)
//     {
//         await using var ms = new MemoryStream();
//         var sut = new StreamBTreeNodeReadWriter(ms, order);
//
//         await sut.WriteNode(node, CancellationToken.None);
//         var readNode = await sut.ReadNode(node.Id, CancellationToken.None);
//
//         var initialJNode = JObject.FromObject(node).ToString();
//         var actualJNode = JObject.FromObject(readNode).ToString();
//
//         Assert.That(actualJNode, Is.EqualTo(initialJNode));
//     }
//
//     private static IEnumerable<TestCaseData> Cases()
//     {
//         yield return new TestCaseData(
//                 new StreamBTreeNode(
//                     new StreamBTreeNodeId(0),
//                     new StreamBTreeNodeId(12),
//                     [new StreamBTreeNodeId(1), new StreamBTreeNodeId(42)],
//                     [new StreamBTreeIndex(3), new StreamBTreeIndex(24), new StreamBTreeIndex(55)]),
//                 new BTreeOrder(1))
//             .SetName("Not empty");
//
//         yield return new TestCaseData(
//                 new StreamBTreeNode(
//                     new StreamBTreeNodeId(0),
//                     null,
//                     [],
//                     []),
//                 new BTreeOrder(10))
//             .SetName("Empty");
//     }
// }