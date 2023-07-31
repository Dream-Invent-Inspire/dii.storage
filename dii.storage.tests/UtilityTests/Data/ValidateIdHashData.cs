using Xunit;

namespace dii.storage.tests.UtilityTests.Data
{
    public class ValidateIdHashData : TheoryData<string, bool>
    {
        public ValidateIdHashData()
        {
            Add("U.G0BFVe2Ag", true);
            Add("U=G0BFVe2Ag", true);
            Add("Abcdefg", true);
            Add(null, false);
            Add(string.Empty, false);
            Add(@"  ", false);
            Add("A bcdefgq43", false);
            Add("; DELETE FROM c", false);
        }
    }
}