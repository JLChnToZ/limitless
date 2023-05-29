using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLChnToZ.CommonUtils.Dynamic.Test {
    internal class TestSubject: IInterface {

        private static string privateStaticStringField = "privateStaticStringField";

        private string privateStringField = "privateStringField";

        private Dictionary<string, string> undelyDictionary = new Dictionary<string, string>();

        private string this[string key] {
            get {
                undelyDictionary.TryGetValue(key, out string value);
                return value;
            }
            set => undelyDictionary[key] = value;
        }

        private static string PrivateStaticStringProperty { get; set; }

        private string PrivateStringProperty { get; set; }

        bool IInterface.HiddenInterfaceProperty { get; set; }

        public TestSubject() {}

        private TestSubject(string testString) {
            privateStringField = testString;
        }


        private static bool PrivateStaticMethod(bool par) {
            return par;
        }

        private static T HiddenStaticGenericMethod<T>(T value) {
            return value;
        }

        private object PrivateMethod(object par) {
            return null;
        }

        private bool PrivateMethod(bool par) {
            return par;
        }

        private int PrivateMethod(int par) {
            return par;
        }

        private T HiddenGenericMethod<T>(T value) {
            return value;
        }

        bool IInterface.HiddenInterfaceMethod(bool par) {
            return par;
        }

        public static IEnumerable<object> EnumerableMethod() {
            for (int i = 0; i < 5; i++)
                yield return new HiddenSubClass();
        }

        static async Task<int> AwaitableMethod(int input) {
            await Task.Delay(100);
            return input;
        }

        static async Task AwaitThrowTask() {
            await Task.Delay(100);
            throw new Exception();
        }

        private struct HiddenSubClass {
            private static string privateStaticStringField = "privateStaticStringField";

            private string privateStringField = "privateStringField";

            public HiddenSubClass() {}
        }
    }

    internal interface IInterface {
        bool HiddenInterfaceProperty { get; set; }

        bool HiddenInterfaceMethod(bool par);
    }
}
