using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI.Basic
{
    public class HotelImageInfoRequest : GenericRequest
    {
        private List<HotelImageInfo> body;

        public List<HotelImageInfo> Body
        {
            get
            {
                return body;
            }

            set
            {
                body = value;
            }
        }
    }
}
