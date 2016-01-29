using DTAPI;

namespace DTAPI.Room
{
    public class RoomInfoRequest : GenericRequest
    {
        private RoomInfoRequestBody body;

        public RoomInfoRequestBody Body
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
