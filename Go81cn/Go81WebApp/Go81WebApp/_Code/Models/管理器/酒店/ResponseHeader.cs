

namespace DTAPI
{
    /// <summary>
    /// 应答的头部
    /// status 应答标识
    /// responseCode 应答/错误代码
    /// responseDescription 应答/错误描述
    /// </summary>
    public class ResponseHeader
    {
        private string status;
        private string responseCode;
        private string responseDescription;

        public string Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
            }
        }

        public string ResponseCode
        {
            get
            {
                return responseCode;
            }

            set
            {
                responseCode = value;
            }
        }

        public string ResponseDescription
        {
            get
            {
                return responseDescription;
            }

            set
            {
                responseDescription = value;
            }
        }
    }
}
