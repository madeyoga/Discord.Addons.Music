using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Addons.Music.Exception
{
    public class TrackErrorException : System.Exception
    {
        public TrackErrorException() : base() { }
        
        public TrackErrorException(string message) : base(message) { }
        
        public TrackErrorException(string message, System.Exception inner) : base(message, inner) { }

        protected TrackErrorException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
