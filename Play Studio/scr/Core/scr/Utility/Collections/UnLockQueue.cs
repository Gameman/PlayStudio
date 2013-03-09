using System.Threading;

namespace System.Collections.Generic
{
    public class UnLockQueue<T>
    {
        static Node<T> _default = new Node<T>(default(T), null);

        private class Node<K>
        {
            internal K Item;
            internal Node<K> Next;

            public Node(K item, Node<K> next)
            {
                this.Item = item;
                this.Next = next;
            }
        }

        private Node<T> _head;
        private Node<T> _tail;

        public UnLockQueue()
        {
            _head = new Node<T>(default(T), null);
            _tail = _head;
        }

        public UnLockQueue<T> Clone() 
        {
            UnLockQueue<T> q = new UnLockQueue<T>();
            q._head = this._head;
            q._tail = this._tail;
            q._count = this._count;
            return q;
        }

        public void Clear() 
        {
            _count = 0;
            _head = new Node<T>(default(T), null);
            _tail = _head;
        }

        int _count;
        public int Count 
        {
            get { return _count; }
        }

        public bool IsEmpty
        {
            get { return (_head.Next == null); }
        }

        public void Enqueue(T item)
        {
            _count++;
            Node<T> newNode = new Node<T>(item, null);
            while (true)
            {
                Node<T> curTail = _tail;
                Node<T> residue = curTail.Next;

                //判断_tail是否被其他process改变
                if (curTail == _tail) {
                    //A 有其他process执行C成功，_tail应该指向新的节点
                    if (residue == null) {
                        //C 如果其他process改变了tail.next节点，需要重新取新的tail节点
                        if (Interlocked.CompareExchange<Node<T>>(
                            ref curTail.Next, newNode, residue) == residue) {
                            //D 尝试修改tail
                            Interlocked.CompareExchange<Node<T>>(ref _tail, newNode, curTail);
                            return;
                        }
                    }
                    else { //B 帮助其他线程完成D操作
                        Interlocked.CompareExchange<Node<T>>(ref _tail, residue, curTail);
                    }
                }
            }

            if (OnEnqueue != null) OnEnqueue(item, EventArgs.Empty);
        }

        public bool TryDequeue(out T result)
        {
            if (_count <= 0)
            {
                result = default(T);
                return false;
            }

            Node<T> curHead;
            Node<T> curTail;
            Node<T> next;
            do {
                curHead = _head;
                curTail = _tail;
                next = curHead.Next;
                if (curHead == _head) {
                    if (next == null)  //Queue为空
                    {
                        result = default(T);
                        return false;
                    }
                    if (curHead == curTail) //Queue处于Enqueue第一个node的过程中
                    {
                        //尝试帮助其他Process完成操作
                        Interlocked.CompareExchange<Node<T>>(ref _tail, next, curTail);
                    }
                    else {
                        //取next.Item必须放到CAS之前
                        result = next.Item;
                        //如果_head没有发生改变，则将_head指向next并退出
                        if (Interlocked.CompareExchange<Node<T>>(ref _head,
                            next, curHead) == curHead)
                            break;
                    }
                }
            }
            while (true);
            _count--;
            return true;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count || IsEmpty) {
                    return default(T);
                }

                if(index == 0) return _head.Item;

                Node<T> result = _head;

                for (int i = 1; i < index; i++)
                    if (result.Next != null)
                        result = result.Next;
                    else return default(T);
                
                return result.Item;
            }
        }

        public event EventHandler OnEnqueue;

    }
}
