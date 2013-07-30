namespace Engine.Misc {
    internal static class Arrays
    {
        internal static T[] InitializeWithDefaultInstances<T>(int length) where T : new()
        {
            var array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = new T();
            }
            return array;
        }
    }
}