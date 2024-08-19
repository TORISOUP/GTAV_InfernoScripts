using System.Collections.Generic;
using Inferno;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfernoTest
{
    [TestClass]
    public class CoroutineSystemTest
    {
        private int actionCount;
        private TestCoroutineSystem testCoroutineSystem;

        [TestInitialize]
        public void Initialize()
        {
            testCoroutineSystem = new TestCoroutineSystem();
        }

        [TestMethod]
        public void AddCoroutine時にそれぞれ別のidが連番で割り振られる()
        {
            for (uint expected = 0; expected < 10; expected++)
            {
                var resultId = testCoroutineSystem.AddCoroutine(testEnumerable(1));
                Assert.AreEqual(expected, resultId);
            }

            //10個登録されている
            Assert.AreEqual(10, testCoroutineSystem.RegisteredCoroutineCount);
        }

        private IEnumerable<object> ExecuteEnumerator()
        {
            actionCount++;
            yield return null;
            actionCount++;
            yield return null;
        }

        [TestMethod]
        public void AddCoroutine時に最初のコルーチンが実行される()
        {
            actionCount = 0;
            testCoroutineSystem.AddCoroutine(ExecuteEnumerator());

            //登録直後に実行されているはず
            Assert.AreEqual(1, actionCount);

            //1回実行
            testCoroutineSystem.CoroutineLoop();

            //実行回数が増えているはず
            Assert.AreEqual(2, actionCount);
        }

        [TestMethod]
        public void RemoveCoroutineで該当のCoroutineが削除される()
        {
            //１０個登録
            for (uint expected = 0; expected < 10; expected++) testCoroutineSystem.AddCoroutine(testEnumerable(5));

            //IDは１０個全て存在する
            Assert.AreEqual(10, testCoroutineSystem.RegisteredCoroutineCount);
            for (uint id = 0; id < 10; id++) Assert.IsTrue(testCoroutineSystem.IsContains(id));

            //偶数のIDを除去
            for (uint id = 0; id < 10; id++)
                if (id % 2 == 0)
                    testCoroutineSystem.RemoveCoroutine(id);

            //除去直後は消えていない
            Assert.AreEqual(10, testCoroutineSystem.RegisteredCoroutineCount);

            //一度コルーチンを回す
            testCoroutineSystem.CoroutineLoop();

            //登録コルーチン数は５個になっている
            Assert.AreEqual(5, testCoroutineSystem.RegisteredCoroutineCount);
            for (uint id = 0; id < 10; id++)
                if (id % 2 == 0)
                    //偶数のキーは存在しない
                    Assert.IsFalse(testCoroutineSystem.IsContains(id));
                else
                    Assert.IsTrue(testCoroutineSystem.IsContains(id));
        }

        [TestMethod]
        public void RemoveCoroutineで存在しないキーを削除しても例外にならない()
        {
            testCoroutineSystem.RemoveCoroutine(0);
        }

        [TestMethod]
        public void ContainsCoroutineで該当のCoroutineが存在するかどうかチェックされる()
        {
            //１０個登録
            for (uint expected = 0; expected < 10; expected++) testCoroutineSystem.AddCoroutine(testEnumerable(5));

            //IDは１０個全て存在するか確認し、すぐに除去
            Assert.AreEqual(10, testCoroutineSystem.RegisteredCoroutineCount);
            for (uint id = 0; id < 10; id++)
            {
                Assert.IsTrue(testCoroutineSystem.ContainsCoroutine(id));
                testCoroutineSystem.RemoveCoroutine(id);
            }

            //一度コルーチンを回す
            testCoroutineSystem.CoroutineLoop();

            //IDは１０個全て存在していない
            Assert.AreEqual(0, testCoroutineSystem.RegisteredCoroutineCount);
            for (uint id = 0; id < 10; id++) Assert.IsFalse(testCoroutineSystem.ContainsCoroutine(id));
        }

        [TestMethod]
        public void NestしたIEnumerableを展開して実行できる()
        {
            testCoroutineSystem.AddCoroutine(nestEnumerable());
            var i = 0;

            //コルーチンが終了するまでの実行回数を数える
            while (testCoroutineSystem.RegisteredCoroutineCount > 0)
            {
                testCoroutineSystem.CoroutineLoop();
                i++;
            }

            Assert.AreEqual(20, i);
        }

        private IEnumerable<object> testEnumerable(int actionCount)
        {
            for (var i = 0; i < actionCount; i++) yield return null;
        }

        private IEnumerable<object> nestEnumerable()
        {
            yield return testEnumerable(5);
            yield return testEnumerable(10);
            yield return testEnumerable(5);
        }
    }

    /// <summary>
    /// テスト用コルーチンシステム
    /// </summary>
    public class TestCoroutineSystem : CoroutineSystem
    {
        public int RegisteredCoroutineCount => _coroutines.Count;

        public bool IsContains(uint Id)
        {
            return _coroutines.ContainsKey(Id);
        }
    }
}