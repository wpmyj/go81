

namespace DTAPI.Room
{
    public class RoomResponse : GenericResponse
    {
        private RoomResponseBody body;

        public RoomResponseBody Body
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
