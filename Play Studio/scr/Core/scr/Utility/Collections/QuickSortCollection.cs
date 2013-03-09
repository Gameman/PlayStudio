using System.Diagnostics;
using System.Collections.ObjectModel;

namespace System.Collections.Generic
{
    public class QuickSortCollection<T> : List<T>, ICollection<T>, IList<T>
    {
        public QuickSortCollection() : base() { }

        public QuickSortCollection(int count) : base(count) { }

        public void Sort()
        {
            var array = Sort(this);
        }

        private QuickSortCollection<T> Sort(QuickSortCollection<T> target)
        {
            if (target != null && target.Count > 1)
            {
                IQuickComparer co = target[target.Count - 1] as IQuickComparer;
                Debug.Assert(co != null, "Quick Sort : haven't find 'IQuickComparer' interface");
                int pivotValue = co.Comparer;
                T temp;
                int i = 0, j = 0;
                for (; i < target.Count - j; i++)
                {
                    co = target[i] as IQuickComparer;
                    Debug.Assert(co != null, "Quick Sort : haven't find 'IQuickComparer' interface");
                    while (co.Comparer > pivotValue)
                    {
                        //若是该数大于中间值，则将其与队尾交换，然后再比较交换过来的值
                        //被交换过来的数，也要通过比较来决定其归属
                        temp = target[i];
                        target[i] = target[target.Count - 1 - j];
                        target[target.Count - 1 - j] = temp;
                        j++;
                        if (i + j == target.Count - 1)
                        {
                            //当前的操作数，和被换走的数，都是大于中间值的，所以加一
                            if (co.Comparer > pivotValue) j++;
                            break;
                        }
                        else
                            //指针正向与反向相遇，不掉换，也不再加一
                            if (i + j >= target.Count) break;
                    }
                }//当循环结束时，后j个元素为大于中间值的数，前面的则小于等于中间值
                //递归的比较,分组前all-j个， 后j个；
                QuickSortCollection<T> front, end;
                //注意：j至少等于1,若j==0,表示中间值即为最大的数，在最后一位，因此可以单分一组
                if (j == 0) j = 1;
                front = new QuickSortCollection<T>(target.Count - j);
                end = new QuickSortCollection<T>(j);
                //copy the value to two new arrays
                for (int num = 0; num < front.Count; num++) front[num] = target[num];
                for (int num = 0; num < end.Count; num++) end[num] = target[front.Count + num];
                //iteration
                front = Sort(front);
                end = Sort(end);
                //copy the value back to two target array
                for (int num = 0; num < front.Count; num++) target[num] = front[num];
                for (int num = 0; num < end.Count; num++) target[front.Count + num] = end[num];
            }
            return target;
        }
    }
}
