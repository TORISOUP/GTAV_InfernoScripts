using System.Collections.Generic;

namespace Inferno.InfernoScripts.InfernoCore.Coroutine
{
    /// <summary>
    /// 複数のコルーチンを順繰りに実行する
    /// </summary>
    class CoroutinePool
    {
        private readonly int _poolSize;
        private int next = 0;
        private readonly CoroutineSystem[] _coroutines;

        /// <summary>
        /// poolのRunを呼び出すべき間隔
        /// 1つあたり100ms間隔になるようにする
        /// </summary>
        public int ExpectExecutionInterbalMillSeconds => 100 / _poolSize;

        public CoroutinePool(int poolSize)
        {
            this._poolSize = poolSize;
            _coroutines = new CoroutineSystem[poolSize];
            for (var i = 0; i < _poolSize; i++)
            {
                _coroutines[i] = new CoroutineSystem();
            }
        }

        /// <summary>
        /// 次のコルーチンを実行する
        /// 実行するとIndexが進む
        /// </summary>
        public void Run()
        {
            _coroutines[next].CoroutineLoop();
            next = (next + 1) % _poolSize;
        }

        /// <summary>
        /// コルーチンを登録する
        /// 次に実行されるIndexのコルーチンに登録される
        /// </summary>
        public uint RegisterCoroutine(IEnumerable<object> coroutine)
        {
            return _coroutines[next].AddCoroutine(coroutine);
        }

        /// <summary>
        /// コルーチンを削除する
        /// </summary>
        public void RemoveCoroutine(uint coroutinId)
        {
            foreach (var c in _coroutines)
            {
                //どこに登録されたかわからないので全部叩く
                c.RemoveCoroutine(coroutinId);
            }
        }

        public void RemoveAllCoroutine()
        {
            foreach (var c in _coroutines)
            {
                c.RemoveAllCoroutine();
            }
        }
    }
}
