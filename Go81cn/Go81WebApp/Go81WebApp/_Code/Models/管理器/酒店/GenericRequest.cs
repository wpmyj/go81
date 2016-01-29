
namespace DTAPI
{
    /// <summary>
    /// 通用请求对象类
    /// </summary>
    public class GenericRequest
    {
        private RequestHeader head;

        public RequestHeader Head
        {
            get
            {
                return head;
            }

            set
            {
                head = value;
            }
        }
    }
}
