using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno
{
    /// <summary>
    /// 自殺する
    /// </summary>
    class KillPlayer:InfernoScript
    {
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("killme")
                .Subscribe(_ => this.GetPlayer().Kill());
        }
    }
}
