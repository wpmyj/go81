using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI.Basic
{
    public class HotelInfoRequest : GenericRequest
    {
        private List<HotelInfo> body;

        public List<HotelInfo> Body
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
