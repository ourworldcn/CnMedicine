using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OW.Data.Entity
{
    public interface IFastChangePosition
    {
        Vector3 Position { get; }

        object ThisLocker { get; }

        bool IsVisible(Vector3 position);
    }

    public static class LinkedListExtensions
    {
        public static void Draw<T>(IEnumerable<T> lSource, IEnumerable<T> rSource, IList<T> lList, IList<T> mList, IList<T> rList, IComparer<T> comparer = null)
        {
            IEnumerator<T> lEnumerator = lSource.OrderBy(c => c, comparer).GetEnumerator();
            var lHasElement = lEnumerator.MoveNext();
            IEnumerator<T> rEnumerator = rSource.OrderBy(c => c, comparer).GetEnumerator();
            var rHasElement = rEnumerator.MoveNext();
            comparer = comparer ?? Comparer<T>.Default;
            int sign;
            bool lDone = false;  //左元素已经处理，空则双方元素都未处理
            bool rDone = false;  //右元素已经处理，空则双方元素都未处理
            while (lHasElement && rHasElement)  //双序列都有元素
            {
                sign = comparer.Compare(lEnumerator.Current, rEnumerator.Current);
                if (sign < 0)  //左元素小于右元素
                {
                    lList.Add(lEnumerator.Current);
                    lHasElement = lEnumerator.MoveNext();
                    lDone = false;
                }
                else if (sign > 0) //若左元素大于右元素
                {
                    rList.Add(rEnumerator.Current);
                    rHasElement = rEnumerator.MoveNext();
                    rDone = false;
                }
                else //两边元素相等
                {
                    if (!lDone)
                        mList.Add(lEnumerator.Current);
                    else if (!rDone)
                        mList.Add(rEnumerator.Current);
                    lHasElement = lEnumerator.MoveNext();
                    lDone = false;
                    rHasElement = rEnumerator.MoveNext();
                    rDone = false;
                }
            }
            if (lHasElement)
            {
                if (!lDone)
                    lList.Add(lEnumerator.Current);
                while (lEnumerator.MoveNext())
                    lList.Add(lEnumerator.Current);
            }
            else if (rHasElement)
            {
                if (!rDone)
                    rList.Add(rEnumerator.Current);
                while (rEnumerator.MoveNext())
                    rList.Add(rEnumerator.Current);
            }
        }

        /// <summary>
        /// 返回序列中满足条件的第一个结点；如果未找到这样的结点，则返回空引用(null)。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="startNode">可以为空引用，将立即返回空引用。</param>
        /// <param name="predicate"></param>
        /// <param name="isPrevious">true则向前搜索，否则(默认)向后搜索。</param>
        /// <returns>满足条件的第一个结点；如果未找到这样的结点，则返回空引用(null)。</returns>
        public static LinkedListNode<T> FirstOrDefault<T>(this LinkedList<T> source, LinkedListNode<T> startNode, Func<T, bool> predicate, bool isPrevious = false)
        {
            LinkedListNode<T> result;
            if (isPrevious)  //若向前
            {
                for (result = startNode; null != result; result = result.Previous)
                {
                    if (predicate(result.Value))
                        break;
                }
            }
            else //若向后
            {
                for (result = startNode; null != result; result = result.Next)
                {
                    if (predicate(result.Value))
                        break;
                }
            }
            return result;
        }

        private static void OrderedJoin<T>(IOrderedEnumerable<T> left, IOrderedEnumerable<T> right, IList<T> l, IList<T> m, IList<T> r)
        {
            IEnumerator<T> lEnumerator = left.GetEnumerator();
            IEnumerator<T> rEnumerator = right.GetEnumerator();
            while (true)
            {
                var lElement = lEnumerator.Current;
                var rElement = rEnumerator.Current;
            }
        }

        /// <summary>
        /// 使用指定的比较器在已排序 链表 的某个元素区域搜索元素，并返回该元素的位置。
        /// 链表必须按<paramref name="comparer"/>已经升序排序，否则行为未知。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">对空的链表，则立即返回空引用。</param>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <param name="hintNode">提示从改结点附近开始搜索。如果是空引用或省略，则从第一个结点开始搜索。</param>
        /// <returns>返回该值应插入的位置，即插入到该结点之前能保持整体有序。空引用说明应追加到链表末尾。</returns>
        public static LinkedListNode<T> Find<T>(this LinkedList<T> source, T item, IComparer<T> comparer, LinkedListNode<T> hintNode = null)
        {
            LinkedListNode<T> tmp;
            if (0 == source.Count)   //若是空链表
                return null;
            if (null == hintNode)   //若从头搜索
            {
                hintNode = source.First;
            }
            var i = comparer.Compare(item, hintNode.Value);
            if (i < 0)  //若向前搜索
            {
                for (tmp = hintNode.Previous; null != tmp; tmp = tmp.Previous)
                    if (comparer.Compare(item, tmp.Value) >= 0)
                        break;

                if (null == tmp)
                    tmp = source.First;
                else
                    tmp = tmp.Next;
            }
            else if (i > 0) //若向后搜索
            {
                for (tmp = hintNode.Next; null != tmp; tmp = tmp.Next)
                    if (comparer.Compare(item, tmp.Value) <= 0)
                        break;
            }
            else //若不变
                return hintNode.Next;
            return tmp;
        }

        /// <summary>
        /// 移动一个节点。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="node">取代该结点的位置。</param>
        /// <param name="moveNode">要移动的结点，可以不在<paramref name="source"/>中，此时会移除后添加到新链表。</param>
        /// <returns>实际发生了移动则返回true,没有移动（无需移动）则返回false。</returns>
        public static bool Move<T>(this LinkedList<T> source, LinkedListNode<T> node, LinkedListNode<T> moveNode)
        {
            if (null == source)
                throw new ArgumentNullException(nameof(source));
            else if (null != node && node.List != source)
                throw new ArgumentException("不是该链表的结点", nameof(node));
            if (moveNode.List == source && (moveNode.Next == node || node == moveNode)) //若无需移动
                return false;
            if (null != moveNode.List)
                moveNode.List.Remove(moveNode);
            if (null == node)
                source.AddLast(moveNode);
            else
                source.AddBefore(node, moveNode);
            return true;
        }
    }

    public class FastChangePositionBase : IFastChangePosition
    {
        public Vector3 Position { get; set; }

        public object ThisLocker => this;

        public bool IsVisible(Vector3 position)
        {
            return true;
        }
    }

    public class MmoCanges<T> where T : IFastChangePosition
    {
        private readonly static Comparer<(Vector3, T)> _Comparer = Comparer<(Vector3, T)>.Create((l, r) =>
        {
            var x = l.Item1.X;
            var y = r.Item1.X;
            if (x < y)
                return -1;
            else if (x > y)
                return 1;
            else
                return 0;
        });

        /// <summary>
        /// 获取与指定结点的笛卡尔距离小于或等于指定值的所有值。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="maxDistance"></param>
        /// <param name="hintNode">提示从该结点附近搜索。</param>
        /// <returns></returns>
        private static IEnumerable<(Vector3, T)> ValuesInRange(Vector3 position, float maxDistance, LinkedListNode<(Vector3, T)> hintNode)
        {
            LinkedListNode<(Vector3, T)> tmp;
            var maxDistanceSquared = maxDistance;   //最大笛卡尔距离的平方
            var minX = position.X - maxDistance;
            var maxX = position.X + maxDistance;

            for (tmp = hintNode; null != tmp && tmp.Value.Item1.X >= minX; tmp = tmp.Previous)
            {
                if (Vector3.DistanceSquared(position, tmp.Value.Item1) <= maxDistanceSquared)
                    yield return tmp.Value;
            }
            for (tmp = hintNode.Next; null != tmp && tmp.Value.Item1.X <= maxX; tmp = tmp.Next)
            {
                if (Vector3.DistanceSquared(position, tmp.Value.Item1) <= maxDistanceSquared)
                    yield return tmp.Value;
            }
            yield break;
        }


        /// <summary>
        /// 最小笛卡尔距离的平方。小于此距离的差异忽略。
        /// </summary>
        float MinDistanceSquared = 0.000001f;

        ConcurrentDictionary<T, LinkedListNode<ValueTuple<Vector3, T>>> _Obj2Nodes = new ConcurrentDictionary<T, LinkedListNode<(Vector3, T)>>();
        LinkedList<(Vector3, T)> _List = new LinkedList<(Vector3, T)>();

        public readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void Change(T changePosition, float maxDistance, List<(Vector3, T)> dispare, List<(Vector3, T)> moved, List<(Vector3, T)> distroy)
        {
            var newPosition = changePosition.Position;
            ValueTuple<Vector3, T> newValue = (newPosition, changePosition);
            Vector3? oldPosition;   //老的位置，空则说明是新增结点
            LinkedListNode<(Vector3, T)> node
                , newBookmark, oldBookmark/*老位置的下一个结点*/;
            bool bTrace,
                isChange;   //结点位置是否变化了
            using (var ch = ObjectPool.TakeObject<ClearHelper>())
            {
                var l = ch.TakeObjectAndReturn<List<(Vector3, T)>>(); l.Clear();
                var r = ch.TakeObjectAndReturn<List<(Vector3, T)>>(); r.Clear();
                //开始可升级读取
                Locker.EnterUpgradeableReadLock();
                try
                {
                    if (!_Obj2Nodes.TryGetValue(changePosition, out node))  //若新增结点
                    {
                        oldBookmark = null;
                        oldPosition = null;
                        node = new LinkedListNode<(Vector3, T)>(newValue);
                        newBookmark = _List.Find(newValue, _Comparer);
                    }
                    else//若非新增结点
                    {
                        oldBookmark = node.Next;
                        oldPosition = node.Value.Item1;
                        if (Vector3.DistanceSquared(newPosition, oldPosition.Value) < MinDistanceSquared)
                            return;
                        newBookmark = _List.Find(newValue, _Comparer, node);
                    }
                    //独占写入
                    Locker.EnterWriteLock();
                    try
                    {
                        isChange = _List.Move(newBookmark, node);
                        if (null == oldPosition)  //若新增结点
                        {
                            bTrace = _Obj2Nodes.TryAdd(changePosition, node);
                            Debug.Assert(bTrace);
                        }
                        else
                            node.Value = newValue;
                    }
                    finally
                    {
                        Locker.ExitWriteLock();
                    }
                    //随后进入共享读区域
                    Locker.EnterReadLock();
                }
                finally
                {
                    Locker.ExitUpgradeableReadLock();
                }
                try
                {
                    l.AddRange(ValuesInRange(newPosition, maxDistance, node).Where(c => c.Item2.IsVisible(newPosition)));
                    if (null != oldPosition)    //若非新增结点
                        r.AddRange(ValuesInRange(oldPosition.Value, maxDistance, oldBookmark ?? _List.Last).Where(c => c.Item2.IsVisible(oldPosition.Value)));
                }
                finally
                {
                    Locker.ExitReadLock();
                }
                l.Remove(newValue); r.Remove(newValue);
                var m = ch.TakeObjectAndReturn<List<(Vector3, T)>>(); m.Clear();
                m.AddRange(l.Intersect(r));
                if (l.Count > 16 || m.Count > 16 || r.Count > 16)
                    Trace.WriteLine($"性能警告：观测实体过多，出现{l.Count}，移动{m.Count},消失{r.Count}");
                dispare.AddRange(l.Except(m));
                distroy.AddRange(r.Except(m));
                moved.AddRange(m);
            }
        }

        void TraceMaxDegreeOfParallelism()
        {
            var RecursiveReadCount = Locker.RecursiveReadCount;
            var RecursiveUpgradeCount = Locker.RecursiveUpgradeCount;
            var RecursiveWriteCount = Locker.RecursiveWriteCount;
            if (RecursiveUpgradeCount > 0 || RecursiveReadCount > 1 || RecursiveWriteCount > 0)
                Interlocked.Increment(ref sd);

        }

        public long sd;

        public void Add(T changePosition, float maxDistance, List<(Vector3, T)> dispare, List<(Vector3, T)> moved, List<(Vector3, T)> distroy)
        {
            Change(changePosition, maxDistance, dispare, moved, distroy);
        }
    }
}
