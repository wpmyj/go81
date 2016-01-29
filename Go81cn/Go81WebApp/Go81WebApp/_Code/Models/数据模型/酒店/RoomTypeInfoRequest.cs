using System;
using System.Collections.Generic;
using System.Linq;


namespace DTAPI.Basic
{
    public class RoomTypeInfoRequest : GenericRequest
    {
        private List<RoomTypeInfo> body;

        public List<RoomTypeInfo> Body
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
