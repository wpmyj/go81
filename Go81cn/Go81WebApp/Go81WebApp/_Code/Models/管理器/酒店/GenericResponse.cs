

namespace DTAPI
{
    /// <summary>
    /// 通用应答对象类
    /// </summary>
    public class GenericResponse
    {
        private ResponseHeader head;

        public ResponseHeader Head
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
