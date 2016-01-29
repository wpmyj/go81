using System;

namespace DTAPI
{
    /// <summary>
    /// 请求的头部
    /// version 版本号 
    /// accountId API帐户
    /// serviceName 调用接口的方法名称
    /// reqTime 当前日期
    /// sign 每次请求时的签名
    /// </summary>
    public class RequestHeader
    {
        private string version;
        private string accountId;
        private string serviceName;
        private string reqTime;
        private string sign;

        /// <summary>
        /// 
        /// </summary>
        public RequestHeader()
        {
            this.version = "1.1.0";
            this.accountId = "jingrui";
            this.serviceName = "";
            this.reqTime = DateTime.Today.ToString("yyyy-MM-dd");
            this.sign = "";
        }

        public string Version
        {
            get
            {
                return version;
            }

            set
            {
                version = value;
            }
        }

        public string AccountId
        {
            get
            {
                return accountId;
            }

            set
            {
                accountId = value;
            }
        }

        public string ServiceName
        {
            get
            {
                return serviceName;
            }

            set
            {
                serviceName = value;
            }
        }

        public string ReqTime
        {
            get
            {
                return reqTime;
            }

            set
            {
                reqTime = value;
            }
        }

        public string Sign
        {
            get
            {
                return sign;
            }

            set
            {
                sign = value;
            }
        }

    }
}
