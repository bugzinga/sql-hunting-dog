
using System;
using System.Collections.Generic;

namespace HuntingDog.DogEngine
{
    public class DependecyResults
    {
        public List<Link> DependsOn = new List<Link>();

        public List<Link> DependantUpon = new List<Link>();
    }

    public class Link
    {
        public String Name
        {
            get;
            set;
        }

        public String Schema
        {
            get;
            set;
        }

        public String Type
        {
            get;
            set;
        }
    }

    public enum Direction
    {
        DependsOn = 1,
        DependentOn = 2,
    }

    public class DatabaseDependencyResult
    {
        public DatabaseSearchResult Obj
        {
            get;
            set;
        }

        public Direction Direction
        {
            get;
            set;
        }
    }
}
