using System.Collections;

namespace System.Collections.Generic
{
    /// <summary>
    /// 逆变清单集合接口
    /// </summary>
    public interface IInList<in T> : IEnumerable
    {
        int     IndexOf(T item);
        void    Insert(int index, T item);
        void    RemoveAt(int index);
       
        void    Add(T item);
        void    Clear();
        bool    Contains(T item);
        void    CopyTo(T[] array, int arrayIndex);
        int     Count { get; }
        bool    IsReadOnly { get; }
        bool    Remove(T item);

    }
}
