using System.Collections.Generic;
using System.Linq;

namespace Engine.Misc {
    public class AutoList <T> {
        private static readonly List<T> Members = new List<T>();

        public static List<T> GetAllMembers() {
            return Members.OfType<T>().ToList();
        }
    }
}