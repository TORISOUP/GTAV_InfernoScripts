using System;
using System.Collections;
using Inferno;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfernoTest
{
    [TestClass]
    public class CoroutineSystemTest
    {
        private TestCoroutineSystem testCoroutineSystem;

        [TestInitialize()]
        public void Initialize()
        {
            testCoroutineSystem = new TestCoroutineSystem();
        }

        [TestMethod]
        public void AddCroutine時にそれぞれ別のidが連番で割り振られる()
        {

            for (uint expected = 0; expected < 10; expected++)
            {
                var resultId = testCoroutineSystem.AddCrotoutine(testEnumerator(1));
                Assert.AreEqual(expected,resultId);
            }

            //10個登録されている
            Assert.AreEqual(10,testCoroutineSystem.RegisteredCoroutineCount);
        }

        [TestMethod]
        public void RemoveCoroutineで該当のCoroutineが削除される()
        {
            //１０個登録
            for (uint expected = 0; expected < 10; expected++)
            {
                testCoroutineSystem.AddCrotoutine(testEnumerator(1));
            }

            //IDは１０個全て存在する
            Assert.AreEqual(10, testCoroutineSystem.RegisteredCoroutineCount);
            for (uint id = 0; id < 10; id++)
            {
                Assert.IsTrue(testCoroutineSystem.IsContains(id));
            }

            //偶数のIDを除去
            for (uint id = 0; id < 10; id++)
            {
                if (id%2 == 0)
                {
                    testCoroutineSystem.RemoveCoroutine(id);
                }
            }

            //登録コルーチン数は５個になっている
            Assert.AreEqual(5, testCoroutineSystem.RegisteredCoroutineCount);
            for (uint id = 0; id < 10; id++)
            {
                if (id%2 == 0)
                {
                    //偶数のキーは存在しない
                    Assert.IsFalse(testCoroutineSystem.IsContains(id));
                }
                else
                {
                    Assert.IsTrue(testCoroutineSystem.IsContains(id));
                }
            }
        }

        [TestMethod]
        public void RemoveCoroutineで存在しないキーを削除しても例外にならない()
        {
            testCoroutineSystem.RemoveCoroutine(0);
        }

        [TestMethod]
        public void CoroutineLoopで終了したコルーチンから削除される()
        {
            //3個登録
            for (var id = 0; id < 3; id++)
            {
                //それぞれ実行回数の違うコルーチンを登録
                testCoroutineSystem.AddCrotoutine(testEnumerator(id));
            }
            //登録コルーチン数は3個になっている
            Assert.AreEqual(3, testCoroutineSystem.RegisteredCoroutineCount);

            //実行
            testCoroutineSystem.CoroutineLoop();

            //登録コルーチン数は2個になっている
            Assert.AreEqual(2, testCoroutineSystem.RegisteredCoroutineCount);

            //実行
            testCoroutineSystem.CoroutineLoop();

            //登録コルーチン数は1個になっている
            Assert.AreEqual(1, testCoroutineSystem.RegisteredCoroutineCount);

            //実行
            testCoroutineSystem.CoroutineLoop();

            //登録コルーチン数は0個になっている
            Assert.AreEqual(0, testCoroutineSystem.RegisteredCoroutineCount);
        }


        private IEnumerator testEnumerator(int actionCount)
        {
            for (int i = 0; i < actionCount; i++)
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// テスト用コルーチンシステム
    /// </summary>
    public class TestCoroutineSystem : CoroutineSystem
    {
        public int RegisteredCoroutineCount
        {
            get { return _coroutines.Count; }
        }

        public bool IsContains(uint Id)
        {
            return _coroutines.ContainsKey(Id);
        }
    }
}
