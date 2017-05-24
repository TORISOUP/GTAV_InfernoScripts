using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        private struct IdCoroutine
        {
            public uint id;
            public IEnumerator coroutine;
        }
        private uint _coroutineIdIndex = 0;
        private readonly object _lockObject = new object();
        private readonly List<uint> _stopCoroutineList = new List<uint>();
        List<uint> endIdList = new List<uint>();
        Queue<IdCoroutine> newCoroutines = new Queue<IdCoroutine>(5);
        private IEnumerator[] _actionCoroutines;

        /// <summary>
        /// コルーチンの登録
        /// </summary>
        /// <param name="coroutine">登録するコルーチン</param>
        /// <returns></returns>
        public uint AddCoroutine(IEnumerable<Object> coroutine)
        {
            lock (_lockObject)
            {
                var id = unchecked(_coroutineIdIndex++);
                //WaitForSecondsを展開できるように
                var enumrator = coroutine
                    .SelectMany(x => x is IEnumerable ? ((IEnumerable<object>)x) : new object[] { x }).GetEnumerator();

                //すぐに追加せずに一旦キューに詰める
                newCoroutines.Enqueue(new IdCoroutine()
                {
                    id = id,
                    coroutine = enumrator
                });
                return id;
            }
        }

        /// <summary>
        /// コルーチンの登録解除
        /// </summary>
        /// <param name="id">解除したいコルーチンID</param>
        public void RemoveCoroutine(uint id)
        {
            lock (_lockObject)
            {
                if (_coroutines.Keys.Contains(id))
                {
                    //このタイミングでは消さない
                    _stopCoroutineList.Add(id);
                }
            }
        }

        /// <summary>
        /// 全てのコルーチンを停止する
        /// </summary>
        public void RemoveAllCoroutine()
        {
            lock (_lockObject)
            {
                _coroutines.Clear();
            }
        }

        /// <summary>
        /// コルーチンが存在するかどうかチェックする
        /// </summary>
        /// <param name="id">存在するかどうか確認したいコルーチンID</param>
        public bool ContainsCoroutine(uint id)
        {
            return _coroutines.ContainsKey(id);
        }

        /// <summary>
        /// コルーチンの処理を行う
        /// </summary>
        public void CoroutineLoop()
        {

            lock (_lockObject)
            {
                //新規追加されたものを追加する
                while (newCoroutines.Count > 0)
                {
                    var n = newCoroutines.Dequeue();
                    _coroutines[n.id] = n.coroutine;
                }


                //開始前に削除登録されたものを消す
                foreach (var stopId in _stopCoroutineList)
                {
                    _coroutines.Remove(stopId);
                }
                _stopCoroutineList.Clear();

                foreach (var coroutine in _coroutines)
                {
                    try
                    {
                        if (!coroutine.Value.MoveNext())
                        {
                            endIdList.Add(coroutine.Key);
                        }
                    }
                    catch (Exception e)
                    {
                        endIdList.Add(coroutine.Key);
                    }
                }
                foreach (var id in endIdList)
                {
                    _coroutines.Remove(id);
                }
                endIdList.Clear();
            }
        }
    }
}
