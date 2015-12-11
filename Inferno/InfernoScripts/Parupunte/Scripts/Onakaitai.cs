using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class Onakaitai : ParupunteScript
    {
        readonly string petroEffect = "ent_sht_petrol";

        private bool AffectAllPed = false;
        private ReduceCounter reduceCounter;
        private List<Ped> targetPeds = new List<Ped>(); 


        public Onakaitai(ParupunteCore core) : base(core)
        {

        }

        public override string Name => AffectAllPed ? "みんなおなかいたい" : "おなかいたい";

        public override void OnSetUp()
        {
            var r = new Random();

            //たまに全員に対して発動させる
            AffectAllPed = r.Next() % 10 == 0;
        }

        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(15000);
            AddProgressBar(reduceCounter);

            var ptfxName = "core";
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }


            
            if (AffectAllPed)
            {
                targetPeds = core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive).ToList();
            }
            targetPeds.Add(core.PlayerPed);

            //コルーチン起動
            foreach (var ped in targetPeds)
            {
                StartCoroutine(OilCoroutine(ped));
            }

            //終わったら着火する
            reduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                foreach (var ped in targetPeds.Where(x=>x.IsSafeExist()&&x.IsAlive))
                {
                    Ignition(ped);
                }

                ParupunteEnd();
            });
        }

        private IEnumerable<object> OilCoroutine(Ped ped)
        {

            while (!reduceCounter.IsCompleted)
            {
                CreateEffect(ped, petroEffect);
                yield return WaitForSeconds(1);
            }
        }

        private void CreateEffect(Ped ped,string effect)
        {
            if(!ped.IsSafeExist()) return;
            var offset = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 3.0f;
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, effect,
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, scale, 0, 0, 0);
        }

        /// <summary>
        /// 点火
        /// </summary>
        private void Ignition(Ped ped)
        {
            if (!ped.IsSafeExist()) return;
            var pos = ped.Position;
            GTA.World.AddOwnedExplosion(ped, pos, GTA.ExplosionType.Bullet, 0.0f, 0.0f);
        }
    }
}
