using Microsoft.CSharp.RuntimeBinder;

namespace JLChnToZ.CommonUtils.Dynamic.Test {
    [TestClass]
    public class LimitlessTest {
        [TestMethod]
        public void TestStatic() {
            var limitless = Limitless.Static(typeof(TestSubject));
            TestLimitless(limitless, true);
            limitless = Limitless.Static("JLChnToZ.CommonUtils.Dynamic.Test.TestSubject, JLChnToZ.CommonUtils.Dynamic.Test");
            TestLimitless(limitless, true);
            limitless = Limitless.Static(limitless);
            TestLimitless(limitless, true);
        }

        [TestMethod]
        public void TestWrap() {
            var testSubject = new TestSubject();
            var limitless = Limitless.Wrap(testSubject);
            TestLimitless(limitless, false);
            limitless = Limitless.Wrap(testSubject, typeof(IInterface));
            TestInterfaceAccess(limitless);
            limitless = Limitless.Wrap(testSubject, "JLChnToZ.CommonUtils.Dynamic.Test.IInterface, JLChnToZ.CommonUtils.Dynamic.Test");
            TestInterfaceAccess(limitless);
            limitless = Limitless.Wrap(Limitless.Wrap(testSubject), "JLChnToZ.CommonUtils.Dynamic.Test.IInterface, JLChnToZ.CommonUtils.Dynamic.Test");
            TestInterfaceAccess(limitless);
            limitless = Limitless.Wrap(1, typeof(TestSubject));
            TestLimitless(limitless, true);
        }

        [TestMethod]
        public void TestOperators() {
            var testSubject = new TestSubject();
            var testSubject2 = new TestSubject();
            var limitless = Limitless.Wrap(testSubject);
            var limitless2 = Limitless.Wrap(testSubject2);
            object _;
            _ = limitless + limitless2;    Assert.AreSame(TestSubject.lastCalledMethod, "operator+",  $"Call operator on 2 wrapped objects + (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless - limitless2;    Assert.AreSame(TestSubject.lastCalledMethod, "operator-",  $"Call operator on 2 wrapped objects - (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless * limitless2;    Assert.AreSame(TestSubject.lastCalledMethod, "operator*",  $"Call operator on 2 wrapped objects * (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless / limitless2;    Assert.AreSame(TestSubject.lastCalledMethod, "operator/",  $"Call operator on 2 wrapped objects / (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless % limitless2;    Assert.AreSame(TestSubject.lastCalledMethod, "operator%",  $"Call operator on 2 wrapped objects % (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless & limitless2;    Assert.AreSame(TestSubject.lastCalledMethod, "operator&",  $"Call operator on 2 wrapped objects & (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless | limitless2;    Assert.AreSame(TestSubject.lastCalledMethod, "operator|",  $"Call operator on 2 wrapped objects | (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless ^ limitless2;    Assert.AreSame(TestSubject.lastCalledMethod, "operator^",  $"Call operator on 2 wrapped objects ^ (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless << 1;            Assert.AreSame(TestSubject.lastCalledMethod, "operator<<", $"Call operator on 2 wrapped objects << (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless >> 1;            Assert.AreSame(TestSubject.lastCalledMethod, "operator>>", $"Call operator on 2 wrapped objects >> (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless++;               Assert.AreSame(TestSubject.lastCalledMethod, "operator++", $"Call operator on 1 wrapped object ++ (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless--;               Assert.AreSame(TestSubject.lastCalledMethod, "operator--", $"Call operator on 1 wrapped object -- (Actual = {TestSubject.lastCalledMethod})");
            _ = +limitless;                Assert.AreSame(TestSubject.lastCalledMethod, "operator+",  $"Call operator on 1 wrapped object + (Actual = {TestSubject.lastCalledMethod})");
            _ = -limitless;                Assert.AreSame(TestSubject.lastCalledMethod, "operator-",  $"Call operator on 1 wrapped object - (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless += limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator+",  $"Call operator on 2 wrapped objects += (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless -= limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator-",  $"Call operator on 2 wrapped objects -= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless *= limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator*",  $"Call operator on 2 wrapped objects *= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless /= limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator/",  $"Call operator on 2 wrapped objects /= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless %= limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator%",  $"Call operator on 2 wrapped objects %= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless &= limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator&",  $"Call operator on 2 wrapped objects &= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless |= limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator|",  $"Call operator on 2 wrapped objects |= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless ^= limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator^",  $"Call operator on 2 wrapped objects ^= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless <<= 1;           Assert.AreSame(TestSubject.lastCalledMethod, "operator<<", $"Call operator on 2 wrapped objects <<= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless >>= 1;           Assert.AreSame(TestSubject.lastCalledMethod, "operator>>", $"Call operator on 2 wrapped objects >>= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless == limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator==", $"Call operator on 2 wrapped objects == (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless != limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator!=", $"Call operator on 2 wrapped objects != (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless > limitless2;    Assert.AreSame(TestSubject.lastCalledMethod, "operator>",  $"Call operator on 2 wrapped objects > (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless < limitless2;    Assert.AreSame(TestSubject.lastCalledMethod, "operator<",  $"Call operator on 2 wrapped objects < (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless >= limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator>=", $"Call operator on 2 wrapped objects >= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless <= limitless2;   Assert.AreSame(TestSubject.lastCalledMethod, "operator<=", $"Call operator on 2 wrapped objects <= (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless + testSubject2;  Assert.AreSame(TestSubject.lastCalledMethod, "operator+",  $"Call operator to unwrapped object + (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless - testSubject2;  Assert.AreSame(TestSubject.lastCalledMethod, "operator-",  $"Call operator to unwrapped object - (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless * testSubject2;  Assert.AreSame(TestSubject.lastCalledMethod, "operator*",  $"Call operator to unwrapped object * (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless / testSubject2;  Assert.AreSame(TestSubject.lastCalledMethod, "operator/",  $"Call operator to unwrapped object / (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless % testSubject2;  Assert.AreSame(TestSubject.lastCalledMethod, "operator%",  $"Call operator to unwrapped object % (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless & testSubject2;  Assert.AreSame(TestSubject.lastCalledMethod, "operator&",  $"Call operator to unwrapped object & (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless | testSubject2;  Assert.AreSame(TestSubject.lastCalledMethod, "operator|",  $"Call operator to unwrapped object | (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless ^ testSubject2;  Assert.AreSame(TestSubject.lastCalledMethod, "operator^",  $"Call operator to unwrapped object ^ (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless == testSubject2; Assert.AreSame(TestSubject.lastCalledMethod, "operator==", $"Call operator to unwrapped object == (Actual = {TestSubject.lastCalledMethod})");
            _ = limitless != testSubject2; Assert.AreSame(TestSubject.lastCalledMethod, "operator!=", $"Call operator to unwrapped object != (Actual = {TestSubject.lastCalledMethod})");
        }

        [TestMethod]
        public void TestConstruct() {
            var limitless = Limitless.Construct(typeof(TestSubject), "test");
            Assert.AreEqual(limitless.privateStringField, "test", "Call constructor");
            TestLimitless(limitless, false);
            limitless = Limitless.Construct("JLChnToZ.CommonUtils.Dynamic.Test.TestSubject, JLChnToZ.CommonUtils.Dynamic.Test", "test2");
            Assert.AreEqual(limitless.privateStringField, "test2", "Call constructor");
            TestLimitless(limitless, false);
        }

        [TestMethod]
        public void TestSubType() {
            var limitless = Limitless.Static(typeof(TestSubject));
            var subType = limitless.HiddenSubClass;
            Assert.IsNotNull(subType, "Get sub type.");
            subType.privateStaticStringField = "test";
            Assert.AreSame(subType.privateStaticStringField, "test", "Access static field from sub type.");
        }

        [TestMethod]
        public void TestEnumerable() {
            var limitless = Limitless.Static(typeof(TestSubject));
            foreach (var entry in limitless.EnumerableMethod()) {
                Assert.IsInstanceOfType(entry, typeof(Limitless), "Enumerate a type.");
                Assert.IsTrue(entry.GetType().Name.EndsWith("HiddenSubClass"), "Enumerate a hidden type.");
                entry.privateStringField = "test3";
                Assert.AreSame(entry.privateStringField, "test3", "Access field from enumerable.");
                break;
            }
        }

        [TestMethod]
        public void TestAwait() => TestAwaitAsync().Wait();

        async Task TestAwaitAsync() {
            var limitless = Limitless.Static(typeof(TestSubject));
            var task = limitless.AwaitableMethod(1);
            Assert.IsInstanceOfType(task, typeof(Limitless), "Get a wrapped task.");
            int result = await task;
            Assert.AreEqual(result, 1, "Await a task.");
            await Assert.ThrowsExceptionAsync<Exception>(() => limitless.AwaitThrowTask(), "Await a task that throws exception.");
        }

        void TestLimitless(dynamic limitless, bool isStatic) {
            TestMethodAccess(limitless, isStatic);
            TestGenericMethodAccess(limitless, isStatic);
            TestPropertyAccess(limitless, isStatic);
            TestFieldAccess(limitless, isStatic);
            if (!isStatic) {
                TestIndexerAccess(limitless);
                TestDelegateWrapping(limitless);
                Assert.IsNotNull((TestSubject)limitless, "Cast to original type.");
            }
        }

        void TestMethodAccess(dynamic limitless, bool isStatic) {
            Assert.IsNotNull(limitless, "Create dynamic object.");
            Assert.IsTrue(limitless.PrivateStaticMethod(true), "Access private static method");
            if (isStatic) return;
            Assert.IsTrue(limitless.PrivateMethod(true), "Access private method");
            Assert.IsNull(limitless.PrivateMethod(null), "Access private method with null");
        }

        void TestGenericMethodAccess(dynamic limitless, bool isStatic) {
            Assert.IsNotNull(limitless);
            Assert.IsTrue(limitless.HiddenStaticGenericMethod.of(typeof(bool))(true), "Access static generic method with typeof.");
            Assert.IsTrue(limitless.HiddenStaticGenericMethod.of("System.Boolean")(true), "Access static generic method with type name.");
            if (isStatic) return;
            Assert.IsTrue(limitless.HiddenGenericMethod.of(typeof(bool))(true), "Access generic method with typeof.");
            Assert.IsTrue(limitless.HiddenGenericMethod.of("System.Boolean")(true), "Access generic method with type name.");
        }

        void TestPropertyAccess(dynamic limitless, bool isStatic) {
            Assert.IsNotNull(limitless);
            limitless.PrivateStaticStringProperty = "test";
            Assert.AreEqual("test", limitless.PrivateStaticStringProperty, "Access static private string property.");
            if (isStatic) return;
            limitless.PrivateStringProperty = "test";
            Assert.AreEqual("test", limitless.PrivateStringProperty, "Access private string property.");
        }

        void TestFieldAccess(dynamic limitless, bool isStatic) {
            Assert.IsNotNull(limitless);
            limitless.privateStaticStringField = "test";
            Assert.AreEqual("test", limitless.privateStaticStringField, "Access static private string field.");
            if (isStatic) return;
            limitless.privateStringField = "test";
            Assert.AreEqual("test", limitless.privateStringField, "Access private string field.");
        }

        void TestIndexerAccess(dynamic limitless) {
            Assert.IsNotNull(limitless);
            limitless["test"] = "test";
            Assert.AreEqual("test", limitless["test"], "Access indexer.");
        }

        void TestDelegateWrapping(dynamic limitless) {
            Func<int, int> testDelegate = limitless.PrivateMethod;
            Assert.IsNotNull(testDelegate, "Wrapping delegate");
            Assert.AreEqual(testDelegate(1), 1, "Invoke wrapped delegate");
        }

        void TestInterfaceAccess(dynamic limitless) {
            Assert.IsNotNull(limitless);
            limitless.HiddenInterfaceProperty = true;
            Assert.IsTrue(limitless.HiddenInterfaceProperty, "Access IInterface.HiddenInterfaceProperty");
            Assert.IsTrue(limitless.HiddenInterfaceMethod(true), "Access IInterface.HiddenInterfaceMethod");
            Assert.ThrowsException<RuntimeBinderException>(() => {
                limitless.privateStringField = "test";
            }, "Access non-interface member");
        }
    }
}
