using RedFox.Graphics3D;
using RedFox.Graphics3D.Skeletal;
using RedFox.Graphics3D.Translation;
using System.Buffers.Text;
using System.Net.Http.Json;
using System.Runtime.Versioning;
using RedFox.Zenith.LicenseVerifiers;
using RedFox.Zenith.LicenseStorages;
using Tpm2Lib;
namespace RedFox.CLI.Sandbox
{
    [SupportedOSPlatform("windows")]
    internal class Program
    {
        static void Main(string[] args)
        {
            var productID = "kfeVcK-dCxEFI2q7O1IySg==";
            var licenseKey1 = "9AB5F1F4-D37B4D40-A4175982-EE3131FA";
            var licenseKey2 = "9AB5F1F4-D37B4D40-A4175982-EE3031FA";

            var storage = new RegistryDataStorage("Scobalula\\Alchemist", "MarvsFinalShit");
            var client = new HttpClient();
            var verifier = new GumRoadLicenseVerifier(storage, client);

            var tpmDevice = new TbsDevice();
            tpmDevice.Connect();
            var tpm = new Tpm2(tpmDevice);

            //TkHashcheck validation;
            //byte[] hashData = tpm.Hash(new byte[] { 1, 2, 3 },   // Data to hash
            //                           TpmAlgId.,          // Hash algorithm
            //                           TpmRh.Owner,              // Hierarchy for ticket (not used here)
            //                           out validation);          // Ticket (not used in this example)

            //Console.WriteLine("Hashed data (Hash): " + BitConverter.ToString(hashData));


            Console.WriteLine(verifier.LicenseKeyExists());

            Console.WriteLine(verifier.ActivateLicenseAsync(productID, licenseKey2).Result);

            Console.WriteLine(DateTime.Parse(storage.RetrieveData("ZenithDate", DateTime.Now.ToString(), true)));

            //storage.StoreData("ZenithData", licenseKey, true);
            //Console.WriteLine(storage.RetrieveData("ZenithData", "None", true));
            //var factory = new Graphics3DTranslatorFactory().WithDefaultTranslators();

            //var skeleton = factory.Load<Skeleton>(@"F:\Tools\JohnMadma\Dobermann\exported_files_all\modern_warfare_4\xmodels\mp_western_vm_arms_domino_1_1\mp_western_vm_arms_domino_1_1_LOD0.semodel");
            //var anim = factory.Load<SkeletonAnimation>(@"F:\Tools\JohnMadma\Weapons\exported_files_all\modern_warfare_4\xanims\vm_ar_mike4_idle.seanim");
            //var anim2 = factory.Load<SkeletonAnimation>(@"F:\Tools\JohnMadma\Weapons\exported_files_all\modern_warfare_4\xanims\vm_ar_mike4_pose_gripvert.seanim");

            //var newAnim = new SkeletonAnimation(anim.Name, anim.Skeleton, 256, TransformType.Absolute);
            //var sampler = new SkeletonAnimationSampler(anim.Name, anim, skeleton);
            //var sampler2 = new SkeletonAnimationSampler(anim.Name, anim2, skeleton);

            //foreach (var subSampler in sampler.TargetSamplers)
            //{
            //    newAnim.Targets.Add(new(subSampler.Bone.Name));
            //}

            //var frameCount = sampler.FrameCount;

            //for (int i = 0; i < frameCount; i++)
            //{
            //    sampler.Update(i, AnimationSampleType.AbsoluteFrameTime);
            //    sampler2.Update(0, AnimationSampleType.AbsoluteFrameTime);

            //    for (int j = 0; j < sampler.TargetSamplers.Count; j++)
            //    {
            //        if (sampler.TargetSamplers[j].Target != null)
            //        {
            //            newAnim.Targets[j].AddTranslationFrame(i, sampler.TargetSamplers[j].Bone.CurrentLocalTranslation);
            //            newAnim.Targets[j].AddRotationFrame(i, sampler.TargetSamplers[j].Bone.CurrentLocalRotation);
            //        }
            //    }
            //}

            ////// Set up, our positions are going to be absolute
            ////newSkelAnim.TransformSpace = newSpace;
            ////newSkelAnim.TransformType = TransformType.Absolute;

            //factory.Save("yo.seanim", newAnim);
        }
    }
}
