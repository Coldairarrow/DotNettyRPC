using System.Threading.Tasks;

namespace Coldairarrow.Util
{
    internal static partial class Extention
    {
        public static T RunSync<T>(this Task<T> task)
        {
            return AsyncHelpers.RunSync(() => task);
        }
    }
}
