using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Crawler
{
    class BinomialHeap<TKey, TValue> where TKey : IComparable
    {
        private IList<BinomialTree<TKey, TValue>> rootList;

        public TValue Minimum
        {
            get
            {
                return MinimumRootTree().RootValue;
            }
        }

        public BinomialHeap()
        {
            rootList = new List<BinomialTree<TKey, TValue>>();
        }

        public BinomialHeap(TKey key, TValue value)
        {
            rootList = new List<BinomialTree<TKey, TValue>> { new BinomialTree<TKey, TValue>(key, value) };
        }

        public BinomialHeap(params BinomialTree<TKey, TValue>[] rootList)
        {
            this.rootList = new List<BinomialTree<TKey, TValue>>(rootList);
        }

        public void Merge(BinomialHeap<TKey, TValue> other)
        {
            List<BinomialTree<TKey, TValue>> oldRoots = rootList.Concat(other.rootList).OrderBy(tree => tree.Order).ToList();
            rootList = new List<BinomialTree<TKey, TValue>>();

            BinomialTree<TKey, TValue> currentTree = null;

            foreach(BinomialTree<TKey, TValue> tree in oldRoots)
            {
                if (currentTree == null)
                {
                    currentTree = tree;
                }
                else
                {
                    if (currentTree.Order < tree.Order)
                    {
                        rootList.Add(currentTree);
                        currentTree = tree;
                    }
                    else
                    {
                        currentTree = MergeTrees(currentTree, tree);
                    }
                }
            }

            if (currentTree != null)
            {
                rootList.Add(currentTree);
            }
        }

        private BinomialTree<TKey, TValue> MergeTrees(BinomialTree<TKey, TValue> tree1, BinomialTree<TKey, TValue> tree2)
        {
            if (tree1.RootKey.CompareTo(tree2.RootKey) <= 0)
            {
                tree1.AddSubTree(tree2);
                return tree1;
            }
            else
            {
                tree2.AddSubTree(tree1);
                return tree2;
            }
        }

        public void Insert(TKey key, TValue value)
        {
            Merge(new BinomialHeap<TKey,TValue>(key, value));
        }

        private BinomialTree<TKey, TValue> MinimumRootTree()
        {
            return rootList.OrderBy(tree => tree.RootKey).First();
        }

        public KeyValuePair<TKey, TValue> DeleteMinimum()
        {
            BinomialTree<TKey, TValue> minimumRootTree = MinimumRootTree();

            rootList.Remove(minimumRootTree);
            Merge(new BinomialHeap<TKey, TValue>(minimumRootTree.SubTrees));

            return minimumRootTree.Root;
        }
    }
}
