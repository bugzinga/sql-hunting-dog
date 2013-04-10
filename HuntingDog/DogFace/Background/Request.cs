
using System;

namespace HuntingDog.DogFace.Background
{
    public class Request
    {
        public Object Argument
        {
            get;
            set;
        }

        public Int32 RequestType
        {
            get;
            set;
        }

        public BackgroundProcessor.DoWork DoWorkFunction
        {
            get;
            set;
        }
    }
}
