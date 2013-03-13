using System.Threading;

namespace Play.Studio.Core.Command.TaskPool.Threading.Command
{
    public interface IHasPriority
    {
        ThreadPriority Priority { get; }
    }
}
