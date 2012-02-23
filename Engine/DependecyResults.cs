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
        public string Name { get; set; }
        public string Schema { get; set; }
        public string Type { get; set; }
    }


    public enum Direction
    {
        DependsOn = 1,
        DependentOn = 2,
    }


    public class DatabaseDependencyResult
    {
        public DatabaseSearchResult Obj { get; set; }
        public Direction Direction { get; set; }
    }
}