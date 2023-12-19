namespace Developer.GeneralGameDevKit.TagSystem
{
    public class DevKitTag
    {
        private readonly string[] _tagStructure;
        private readonly string _fullPathOfTag;

        public DevKitTag(string fullPathOfTag)
        {
            _fullPathOfTag = fullPathOfTag;
            _tagStructure = fullPathOfTag.Split('/');
        }

        /// <summary>
        /// Returns single string corresponding to the entire path. 
        /// </summary>
        /// <returns></returns>
        public string GetFullPathOfTag() => _fullPathOfTag;
        
        /// <summary>
        /// Returns string array that make up this tag.
        /// </summary>
        /// <returns>array of strings that make up this tag.</returns>
        public string[] GetTagStructure() => _tagStructure;


        /// <summary>
        /// Check this tag is sub tag of other tag.
        /// </summary>
        /// <param name="otherTag">Tag to compare</param>
        /// <returns>Result of sub tag test</returns>
        public bool IsSubTagOf(DevKitTag otherTag)
        {
            var otherTagStructure = otherTag.GetTagStructure();
            if (_tagStructure.Length > otherTagStructure.Length)
            {
                return false;
            }
                
            for (var i = 0; i < _tagStructure.Length && i < otherTagStructure.Length; i++)
            {
                if (!_tagStructure[i].Equals(otherTagStructure[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check this tag is equal with other tag.
        /// </summary>
        /// <param name="otherTag">Tag to compare</param>
        /// <returns>Result of equal tag test</returns>
        public bool IsEqual(DevKitTag otherTag)
        {
            var otherTagStructure = otherTag.GetTagStructure();
            if (otherTagStructure.Length != _tagStructure.Length)
            {
                return false;
            }

            for (var i = 0; i < _tagStructure.Length && i < otherTagStructure.Length; i++)
            {
                if (!_tagStructure[i].Equals(otherTagStructure[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check this tag is sub tag of any member in container.
        /// </summary>
        /// <param name="container">container to compare</param>
        /// <returns>Result of test</returns>
        public bool IsSubTagOfAnyMember(DevKitTagContainer container)
        {
            return container.HasSuperTagAny(this);
        }

        /// <summary>
        /// Check this tag is sub tag of all member in container.
        /// </summary>
        /// <param name="container">container to compare</param>
        /// <returns>Result of test</returns>
        public bool IsSubTagOfAllMember(DevKitTagContainer container)
        {
            return container.HasSuperTagAll(this);
        }

        /// <summary>
        /// Check this tag is exact member in container.
        /// </summary>
        /// <param name="container">container to compare</param>
        /// <returns>Result of test</returns>
        public bool IsExactMemberOf(DevKitTagContainer container)
        {
            return container.HasExactTag(this);
        }
    }
}