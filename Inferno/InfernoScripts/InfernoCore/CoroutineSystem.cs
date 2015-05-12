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
        private List<uint> stopCoroutineList = new List<uint>(); 

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
            //このタイミングでは消さない
            stopCoroutineList.Add(id);
        }

        /// <summary>
        /// コルーチン処理
        /// </summary>
        public void CoroutineLoop()
        {
            lock (lockObject)
            {
                //開始前に削除登録されたものを消す
                foreach (var stopId in stopCoroutineList.ToArray())
                {
                    _coroutines.Remove(stopId);
                }
                stopCoroutineList.Clear();

                var endIdList = new List<uint>();

                foreach (var coroutine in _coroutines.ToArray())
                {
                    if (!coroutine.Value.MoveNext())
                    {
                        endIdList.Add(coroutine.Key);
                    }
                }

                foreach (var id in endIdList.ToArray())
                {
                    _coroutines.Remove(id);
                }
            }
        }
    }
}
