using System;
using System.Collections.Generic;
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
                var resultId = testCoroutineSystem.AddCrotoutine(testEnumerable(1));
                Assert.AreEqual(expected, resultId);
            }

            //10個登録されている
            Assert.AreEqual(10, testCoroutineSystem.RegisteredCoroutineCount);
        }

        private int actionCount = 0;

        IEnumerable<Object> ExecuteEnumerator()
        {
            actionCount++;
            yield return null;
            actionCount++;
            yield return null;
        }


        [TestMethod]
        public void AddCroutine時に最初のコルーチンが実行される()
        {
            actionCount = 0;
            testCoroutineSystem.AddCrotoutine(ExecuteEnumerator());

            //登録直後に実行されているはず
            Assert.AreEqual(1, actionCount);

            //1回実行
            testCoroutineSystem.CoroutineLoop(0);

            //実行回数が増えているはず
            Assert.AreEqual(2, actionCount);
        }

        [TestMethod]
        public void RemoveCoroutineで該当のCoroutineが削除される()
        {
            //１０個登録
            for (uint expected = 0; expected < 10; expected++)
            {
                testCoroutineSystem.AddCrotoutine(testEnumerable(5));
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
                if (id % 2 == 0)
                {
                    testCoroutineSystem.RemoveCoroutine(id);
                }
            }

            //除去直後は消えていない
            Assert.AreEqual(10, testCoroutineSystem.RegisteredCoroutineCount);

            //一度コルーチンを回す
            testCoroutineSystem.CoroutineLoop(0);

            //登録コルーチン数は５個になっている
            Assert.AreEqual(5, testCoroutineSystem.RegisteredCoroutineCount);
            for (uint id = 0; id < 10; id++)
            {
                if (id % 2 == 0)
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
        public void ShardIdを指定してコルーチンを回すことができる()
        {
            //4個登録
            for (var id = 0; id < 4; id++)
            {
                //それぞれ1回実行したら終了
                testCoroutineSystem.AddCrotoutine(testEnumerable(1));
            }
            //登録コルーチン数は4個になっている
            Assert.AreEqual(4, testCoroutineSystem.RegisteredCoroutineCount);

            //shardId=0で実行
            testCoroutineSystem.CoroutineLoop(0);

            //登録コルーチン数は3個になっている
            Assert.AreEqual(3, testCoroutineSystem.RegisteredCoroutineCount);

            //shardId=1で実行
            testCoroutineSystem.CoroutineLoop(1);

            //登録コルーチン数は2個になっている
            Assert.AreEqual(2, testCoroutineSystem.RegisteredCoroutineCount);

            //shardId=2で実行
            testCoroutineSystem.CoroutineLoop(2);

            //登録コルーチン数は1個になっている
            Assert.AreEqual(1, testCoroutineSystem.RegisteredCoroutineCount);

            //shardId=3で実行
            testCoroutineSystem.CoroutineLoop(3);

            //登録コルーチン数は0個になっている
            Assert.AreEqual(0, testCoroutineSystem.RegisteredCoroutineCount);
        }


        [TestMethod]
        public void NestしたIEnumerableを展開して実行できる()
        {
            testCoroutineSystem.AddCrotoutine(nestEnumerable());
            var i = 0;

            //コルーチンが終了するまでの実行回数を数える
            while (testCoroutineSystem.RegisteredCoroutineCount > 0)
            {
                testCoroutineSystem.CoroutineLoop(0);
                i++;
            }

            Assert.AreEqual(20, i);
        }

        private IEnumerable<Object> testEnumerable(int actionCount)
        {
            for (int i = 0; i < actionCount; i++)
            {
                yield return null;
            }
        }

        private IEnumerable<Object> nestEnumerable()
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
