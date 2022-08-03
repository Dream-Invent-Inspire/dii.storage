using System;

namespace dii.storage.cosmos.tests.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TestCollectionPriorityOrderAttribute : Attribute
    {
        public TestCollectionPriorityOrderAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; private set; }
    }
}