using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Inferno.ChaosMode;

namespace InfernoTest
{
    /// <summary>
    /// ChaosModeSettingLoaderTest の概要の説明
    /// </summary>
    [TestClass]
    public class ChaosModeSettingLoaderTest
    {       
        [TestMethod]
        public void TestMethod1()
        {
            var test = new TestLoader("aaa");
        }

        /// <summary>
        /// テスト用ローダー
        /// </summary>
        class TestLoader : ChaosModeSettingLoader
        {
            private readonly string _readJson;

            public TestLoader(string readJson)
            {
                _readJson = readJson;
            }

            new string ReadFile(string filePath)
            {
                return _readJson;
            }
        }
    }
}
