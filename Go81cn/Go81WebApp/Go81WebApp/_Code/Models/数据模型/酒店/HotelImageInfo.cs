using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI.Basic
{
    public class HotelImageInfo
    {
        private string hotelCode;
        private string picName;
        private string picUrl;
        private int licensedDownload;

        public string HotelCode
        {
            get
            {
                return hotelCode;
            }

            set
            {
                hotelCode = value;
            }
        }

        public string PicName
        {
            get
            {
                return picName;
            }

            set
            {
                picName = value;
            }
        }

        public string PicUrl
        {
            get
            {
                return picUrl;
            }

            set
            {
                picUrl = value;
            }
        }

        public int LicensedDownload
        {
            get
            {
                return licensedDownload;
            }

            set
            {
                licensedDownload = value;
            }
        }
    }
}
