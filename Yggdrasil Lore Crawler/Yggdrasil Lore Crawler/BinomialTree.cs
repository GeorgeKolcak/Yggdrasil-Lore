using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yggdrasil_Lore_Crawler
{
    class BinomialTree<TKey, TValue> where TKey : IComparable
    {
        public KeyValuePair<TKey, TValue> Root { get; private set; }

        private IList<BinomialTree<TKey, TValue>> subTrees;

        public TKey RootKey
        {
            get
            {
                return Root.Key;
            }
        }

        public TValue RootValue
        {
            get
            {
                return Root.Value;
            }
        }

        public int Order
        {
            get
            {
                return subTrees.Count;
            }
        }

        public BinomialTree<TKey, TValue>[] SubTrees
        {
            get
            {
                return subTrees.ToArray();
            }
        }

        public BinomialTree(TKey rootKey, TValue rootValue)
        {
            Root = new KeyValuePair<TKey, TValue>(rootKey, rootValue);

            subTrees = new List<BinomialTree<TKey, TValue>>();
        }

        public void AddSubTree(BinomialTree<TKey, TValue> subTree)
        {
            subTrees.Add(subTree);
        }

        public override bool Equals(object obj)
        {
            BinomialTree<TKey, TValue> other;
            if ((other = obj as BinomialTree<TKey, TValue>) == null)
            {
                return false;
            }

            if (!RootValue.Equals(other.RootValue) || (RootKey.CompareTo(other.RootKey) != 0) || (subTrees.Count != other.subTrees.Count))
            {
                return false;
            }

            foreach (BinomialTree<TKey, TValue> subTree in subTrees)
            {
                if (!other.subTrees.Contains(subTree))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = (113 + (11 * RootValue.GetHashCode()) + (17 * RootKey.GetHashCode()) + (31 * subTrees.Count));

            for (int i = 0; i < subTrees.Count; i++ )
            {
                hash += ((int)Math.Pow(73, i) * subTrees[i].GetHashCode());
            }

            return hash;
        }
    }
}
