using FSO.Files.XA;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using FSO.HIT.Model;

namespace FSO.HIT
{
    /// <summary>
    /// HIT player responsible for playing lot ambience
    /// </summary>
    public class AmbiencePlayer
    {
        private bool fscMode;
        private FSCPlayer fsc;
        private SoundEffect sfx;
        private SoundEffectInstance inst;

        /// <summary>
        /// Create an ambience player instance and play provided ambience
        /// </summary>
        /// <param name="amb">Ambience to play</param>
        public AmbiencePlayer(Ambience amb)
        {
            if (amb.Loop)
            {
                byte[] data = new XAFile(FSO.Content.Content.Get().GetPath(amb.Path)).DecompressedData;
                var stream = new MemoryStream(data);
                sfx = SoundEffect.FromStream(stream);
                stream.Close();

                inst = sfx.CreateInstance();
                inst.IsLooped = true;
                inst.Volume = HITVM.Get().GetMasterVolume(HITVolumeGroup.AMBIENCE);
                inst.Play();
                HITVM.Get().AmbLoops.Add(inst);

                fscMode = false;
            }
            else
            {
                fsc = HITVM.Get().PlayFSC(FSO.Content.Content.Get().GetPath(amb.Path));
                fsc.SetVolume(0.33f); //may need tweaking
                fscMode = true;
            }
        }

        /// <summary>
        /// Stop and destroy ambiance
        /// </summary>
        public void Kill()
        {
            if (fscMode)
                HITVM.Get().StopFSC(fsc);
            else
            {
                inst.Stop();
                inst.Dispose();
                HITVM.Get().AmbLoops.Remove(inst);
                sfx.Dispose();
            }
        }
    }

    /// <summary>
    /// Contains all information about ambience to play
    /// </summary>
    public struct Ambience
    {
        // Path to ambience's .xa file
        public string Path;
        // Control looping, certain ambiences are simple xa loops instead of fscs.
        public bool Loop;
        
        public Ambience(string path, bool loop)
        {
            Path = path;
            Loop = loop;
        }
    }
}
