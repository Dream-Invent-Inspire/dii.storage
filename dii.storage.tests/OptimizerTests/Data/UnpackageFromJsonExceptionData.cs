using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests.Data
{
    public class UnpackageFromJsonExceptionData : TheoryData<string, Type, string>
    {
        public UnpackageFromJsonExceptionData()
        {
            Add(null, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'json')");
            Add(string.Empty, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'json')");
            Add(@"   ", typeof(ArgumentNullException), "Value cannot be null. (Parameter 'json')");
            Add("{ }", typeof(ArgumentException), "Packed object contained no properties. (Parameter 'packedObject')");
        }
    }
}