using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HuntingDog.DogEngine;

namespace WinForms
{
    public class TestServer : IServer, IEquatable<TestServer>
    {

        public TestServer(string name)
        {
            ServerName = name;
            ID = name;
        }

        public string ServerName
        {
            get;
            set;
        }

        public string ID
        {
            get;
            set;
        }



        public bool Equals(TestServer other)
        {
            if (other == null)
                return false;
            return string.Compare(other.ID, this.ID, true) == 0;
        }


 
    }
}
