
using System;

namespace HuntingDog.DogEngine
{
    public class ProcedureParameter : FunctionParameter
    {
        public String DefaultValue
        {
            get;
            set;
        }

        public Boolean IsOut
        {
            get;
            set;
        }
    }
}
