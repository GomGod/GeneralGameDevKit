using System.Collections.Generic;
using System.Linq;

namespace Developer.GeneralGameDevKit.TagSystem
{
    public class DevKitTagContainer
    {
        private List<DevKitTag> _tagCollection = new();

        public bool HasSuperTagAny(DevKitTag tag)
        {
            return _tagCollection.Any(tag.IsSubTagOf);
        }

        public bool HasSuperTagAll(DevKitTag tag)
        {
            return _tagCollection.All(tag.IsSubTagOf);
        }

        public bool HasExactTag(DevKitTag tag)
        {
            return _tagCollection.Any(tag.IsEqual);
        }

        public bool IsSubContainerOf(DevKitTagContainer container)
        {
            return _tagCollection.All(container.HasExactTag);
        }
    }
}