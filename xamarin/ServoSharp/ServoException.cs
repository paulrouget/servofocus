using System;

namespace ServoSharp
{
    public class ServoException : Exception
    {
        public ServoResult ServoResult { get; }

        public ServoException(ServoResult servoResult)
        {
            ServoResult = servoResult;
        }
    }
}