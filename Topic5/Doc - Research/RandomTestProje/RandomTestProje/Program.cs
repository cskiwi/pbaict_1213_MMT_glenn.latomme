using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomTestProje {
    internal class Program {
        private static void Main(string[] args) {
            var testProgram = new TestProgram();
            var test = new List<BaseObject> {
                new InheretedObjectA("Hello From list"),
                new InheretedObjectA("Hello2 From list"),
                new InheretedObjectB("Hello3 From list"),
                new InheretedObjectB("Hello4 From list")
            };


            var aobj = new InheretedObjectA("Hello");
            var aobj2 = new InheretedObjectA("Boe");
            var aobj3 = new InheretedObjectA("Hi");

            var bobj = new InheretedObjectB("Hello");
            var bobj2 = new InheretedObjectB("Boe");
            var bobj3 = new InheretedObjectB("Hi");

            testProgram.FindMembers();

            Console.ReadKey();
        }
    }

    #region Objects

    public class BaseObject {
        public string Name { get; set; }
        public string WelcomeMessage { get; set; }

        public override string ToString() {
            return Name + ": " + WelcomeMessage;
        }
    }

    public class InheretedObjectA : BaseObject {
        public InheretedObjectA(string welcomeText) {
            Name = "A";
            WelcomeMessage = welcomeText;
        }
    }

    internal class InheretedObjectB : BaseObject {
        public InheretedObjectB(string welcomeText) {
            Name = "B";
            WelcomeMessage = welcomeText;
        }
    }

    #endregion Objects

    #region TestClass

    internal class TestProgram {
        public void FindMembers() {
            List<BaseObject> objects = AutoList<BaseObject>.GetOFType(this);
            foreach (BaseObject b in objects)
                Console.WriteLine(b.ToString());
        }
    }

    #endregion TestClass

    #region Autolist

    /// <summary>
    ///     Autolist class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AutoList<T> {
        private static readonly List<T> Members = new List<T>();

        public static List<T> GetAllMembers() {
            return Members.OfType<T>().ToList();
        }

        IEnumerable<T> GetOFType<T>(IEnumerable<T> list){
            return list.Where(p => p.GetType() == typeof(T));
        }
    }

    #endregion Autolist
}