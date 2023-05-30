namespace JLChnToZ.CommonUtils.Dynamic.Test {
    internal class TestSubject: IInterface {
        public static string lastCalledMethod;

        private static string privateStaticStringField = "privateStaticStringField";

        private string privateStringField = "privateStringField";

        private Dictionary<string, string> undelyDictionary = new Dictionary<string, string>();

        private string this[string key] {
            get {
                lastCalledMethod = "this.get";
                undelyDictionary.TryGetValue(key, out string value);
                return value;
            }
            set {
                lastCalledMethod = "this.set";
                undelyDictionary[key] = value;
            }
        }

        private static string PrivateStaticStringProperty {
            get {
                lastCalledMethod = "PrivateStaticStringProperty.get";
                return privateStaticStringField;
            }
            set {
                lastCalledMethod = "PrivateStaticStringProperty.set";
                privateStaticStringField = value;
            }
        }

        private string PrivateStringProperty {
            get {
                lastCalledMethod = "PrivateStringProperty.get";
                return privateStringField;
            }
            set {
                lastCalledMethod = "PrivateStringProperty.set";
                privateStringField = value;
            }
        }


        bool hiddenInterfacePropertyValue;
        bool IInterface.HiddenInterfaceProperty {
            get {
                lastCalledMethod = "HiddenInterfaceProperty.get";
                return hiddenInterfacePropertyValue;
            }
            set {
                lastCalledMethod = "HiddenInterfaceProperty.set";
                hiddenInterfacePropertyValue = value;
            }
        }

        public TestSubject() {
            lastCalledMethod = "ctor0";
        }

        private TestSubject(string testString) {
            lastCalledMethod = "ctor1";
            privateStringField = testString;
        }


        private static bool PrivateStaticMethod(bool par) {
            lastCalledMethod = "PrivateStaticMethodBool";
            return par;
        }

        private static T HiddenStaticGenericMethod<T>(T value) {
            lastCalledMethod = "HiddenStaticGenericMethodT";
            return value;
        }

        private object PrivateMethod(object par) {
            lastCalledMethod = "PrivateMethodObject";
            return null;
        }

        private bool PrivateMethod(bool par) {
            lastCalledMethod = "PrivateMethodBool";
            return par;
        }

        private int PrivateMethod(int par) {
            lastCalledMethod = "PrivateMethodInt";
            return par;
        }

        private T HiddenGenericMethod<T>(T value) {
            lastCalledMethod = "HiddenGenericMethodT";
            return value;
        }

        bool IInterface.HiddenInterfaceMethod(bool par) {
            lastCalledMethod = "HiddenInterfaceMethodBool";
            return par;
        }

        public static IEnumerable<object> EnumerableMethod() {
            lastCalledMethod = "EnumerableMethod";
            for (int i = 0; i < 5; i++)
                yield return new HiddenSubClass();
        }

        static async Task<int> AwaitableMethod(int input) {
            lastCalledMethod = "AwaitableMethod";
            await Task.Delay(100);
            return input;
        }

        static async Task AwaitThrowTask() {
            lastCalledMethod = "AwaitThrowTask";
            await Task.Delay(100);
            throw new Exception();
        }
        public static TestSubject operator +(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator+";
            return a;
        }

        public static TestSubject operator -(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator-";
            return a;
        }

        public static TestSubject operator *(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator*";
            return a;
        }

        public static TestSubject operator /(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator/";
            return a;
        }

        public static TestSubject operator %(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator%";
            return a;
        }

        public static TestSubject operator &(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator&";
            return a;
        }

        public static TestSubject operator |(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator|";
            return a;
        }

        public static TestSubject operator ^(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator^";
            return a;
        }

        public static TestSubject operator <<(TestSubject a, int b) {
            lastCalledMethod = "operator<<";
            return a;
        }

        public static TestSubject operator >>(TestSubject a, int b) {
            lastCalledMethod = "operator>>";
            return a;
        }

        public static TestSubject operator ~(TestSubject a) {
            lastCalledMethod = "operator~";
            return a;
        }

        public static TestSubject operator !(TestSubject a) {
            lastCalledMethod = "operator!";
            return a;
        }

        public static TestSubject operator ++(TestSubject a) {
            lastCalledMethod = "operator++";
            return a;
        }

        public static TestSubject operator --(TestSubject a) {
            lastCalledMethod = "operator--";
            return a;
        }

        public static TestSubject operator +(TestSubject a) {
            lastCalledMethod = "operator+";
            return a;
        }

        public static TestSubject operator -(TestSubject a) {
            lastCalledMethod = "operator-";
            return a;
        }

        public static bool operator ==(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator==";
            return true;
        }

        public static bool operator !=(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator!=";
            return true;
        }

        public static bool operator <(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator<";
            return true;
        }

        public static bool operator >(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator>";
            return true;
        }

        public static bool operator <=(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator<=";
            return true;
        }

        public static bool operator >=(TestSubject a, TestSubject b) {
            lastCalledMethod = "operator>=";
            return true;
        }

        public static implicit operator TestSubject(int a) {
            var obj = new TestSubject();
            lastCalledMethod = "operator int";
            return obj;
        }

        public static explicit operator string(TestSubject a) {
            lastCalledMethod = "operator string";
            return "test";
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
