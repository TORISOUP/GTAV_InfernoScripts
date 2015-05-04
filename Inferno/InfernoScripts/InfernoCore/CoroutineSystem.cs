using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Inferno
{
    /// <summary>
    /// コルーチンの動作管理
    /// </summary>
    public class CoroutineSystem
    {
        /// <summary>
        /// コルーチンの辞書
        /// </summary>
        protected Dictionary<uint, IEnumerator> _coroutines = new Dictionary<uint, IEnumerator>();
        private uint coroutineIdIndex = 0;
        private Object lockObject = new object();


        /// <summary>
        /// コルーチンの登録
        /// </summary>
        /// <param name="coroutine">登録するコルーチン</param>
        /// <returns></returns>
        public uint AddCrotoutine(IEnumerator coroutine)
        {
            lock (lockObject)
            {
                var id = coroutineIdIndex++;
                _coroutines.Add(id, coroutine);
                coroutine.MoveNext();
                return id;
            }
        }

        /// <summary>
        /// コルーチンの登録解除
        /// </summary>
        /// <param name="id">解除したいコルーチンID</param>
        public void RemoveCoroutine(uint id)
        {
            lock (lockObject)
            {
                _coroutines.Remove(id);
            }
        }

        /// <summary>
        /// コルーチン処理
        /// </summary>
        public void CoroutineLoop()
        {
            var endIdList = new List<uint>();
            foreach (var coroutine in _coroutines)
            {
                if (!coroutine.Value.MoveNext())
                {
                    endIdList.Add(coroutine.Key);
                }
            }

            foreach (var id in endIdList)
            {
                _coroutines.Remove(id);
            }
        }
    }
}
