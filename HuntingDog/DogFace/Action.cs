
using System;
using HuntingDog.DogEngine;

namespace HuntingDog.DogFace
{
    public class Action
    {
        public String Name
        {
            get;
            set;
        }

        public System.Action<IStudioController, IServer> Routine
        {
            get;
            set;
        }
    }

    public class ServerDatabaseCommand
    {
        public IServer Server { get; set; }
        public string DatabaseName { get; set; }
    }
}
