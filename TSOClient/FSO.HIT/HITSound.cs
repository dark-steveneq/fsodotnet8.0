using FSO.HIT.Model;
using System;
using System.Collections.Generic;

namespace FSO.HIT
{
    /// <summary>
    /// Handle for sound being played by HITVM
    /// </summary>
    public abstract class HITSound : IDisposable
    {
        /// <summary>
        /// Is Emitter3D instance initialized?
        /// </summary>
        protected bool VolumeSet;
        /// <summary>
        /// Self explainotary
        /// </summary>
        protected float Volume = 1;
        /// <summary>
        /// Self explainotary
        /// </summary>
        protected float InstVolume = 1;
        protected float Pan;
        /// <summary>
        /// MonoGame object which plays sounds
        /// </summary>
        protected Microsoft.Xna.Framework.Audio.AudioEmitter Emitter3D;

        /// <summary>
        /// Used to prevent getting killed if the sound didn't have owners (used in UI sounds)
        /// </summary>
        protected bool EverHadOwners;
        /// <summary>
        /// Index of recently referenced owner
        /// </summary>
        public int LastMainOwner = -1;
        /// <summary>
        /// List of IDs of owners, objects which reference this sound
        /// </summary>
        protected List<int> Owners;

        /// <summary>
        /// Did playback finish?
        /// </summary>
        public bool Dead;
        /// <summary>
        /// Reference to parent HITVM instance
        /// </summary>
        public HITVM VM;
        /// <summary>
        /// Volume group used by this sound
        /// </summary>
        public HITVolumeGroup VolGroup;
        /// <summary>
        /// Name of this sound
        /// </summary>
        public string Name;

        /// <summary>
        /// Initialize owner list of instance
        /// </summary>
        public HITSound()
        {
            Owners = new List<int>();
        }

        /// <summary>
        /// Used for checking if should continue playing
        /// </summary>
        /// <returns>Should play?</returns>
        public abstract bool Tick();

        /// <summary>
        /// Set volume of instance
        /// </summary>
        /// <param name="volume">Volume to set</param>
        /// <param name="pan"></param>
        /// <param name="ownerID">ID of owner to affect</param>
        /// <returns>Did succeed?</returns>
        public bool SetVolume(float volume, float pan, int ownerID)
        {
            if (VolumeSet)
            {
                if (volume > InstVolume)
                {
                    if (LastMainOwner != ownerID) 
                        LastMainOwner = ownerID;
                    InstVolume = volume;
                    RecalculateVolume();
                    Pan = pan;
                    return true;
                }
                return false;
            }
            else
            {
                if (LastMainOwner != ownerID)
                    LastMainOwner = ownerID;
                InstVolume = volume;
                RecalculateVolume();
                Pan = pan;
                return true;
            }
        }

        /// <summary>
        /// Mute sound
        /// </summary>
        public void Mute()
        {
            InstVolume = 0;
            Volume = 0;
            VolumeSet = true;
        }
        
        /// <summary>
        /// Update Emmiter3D's position
        /// </summary>
        /// <param name="Position">Vector with position</param>
        public void Set3D(Microsoft.Xna.Framework.Vector3 Position)
        {
            Microsoft.Xna.Framework.Audio.SoundEffect.DistanceScale = 100f;
            if (Emitter3D == null)
                Emitter3D = new Microsoft.Xna.Framework.Audio.AudioEmitter();
            Emitter3D.Position = Position;
        }

        /// <summary>
        /// Applies 3D positioning to the provided SoundEffectInstance
        /// </summary>
        /// <param name="inst">SoundEffectInstance to affect</param>
        public void Apply3D(Microsoft.Xna.Framework.Audio.SoundEffectInstance inst)
        {
            Emitter3D.Forward = VM.Listener.Forward;
            inst.Volume = 1f;
            inst.Apply3D(VM.Listener, Emitter3D);
        }

        /// <summary>
        /// Update and multiply volume times master volume
        /// </summary>
        public void RecalculateVolume()
        {
            VolumeSet = true;
            Volume = InstVolume * GetVolFactor();
        }

        /// <summary>
        /// Get master volume
        /// </summary>
        /// <returns>Master volume</returns>
        public float GetVolFactor()
        {
            return VM?.GetMasterVolume(VolGroup) ?? 1f;
        }

        /// <summary>
        /// Add owner to sound
        /// </summary>
        /// <param name="id">Owner ID</param>
        public void AddOwner(int id)
        {
            EverHadOwners = true;
            Owners.Add(id);
        }

        /// <summary>
        /// Remove owner from sound
        /// </summary>
        /// <param name="id">Owner ID</param>
        public void RemoveOwner(int id)
        {
            Owners.Remove(id);
        }

        /// <summary>
        /// Check if owner owns this sound
        /// </summary>
        /// <param name="id">Owner ID</param>
        /// <returns>Is owned?</returns>
        public bool AlreadyOwns(int id)
        {
            return Owners.Contains(id);
        }

        /// <summary>
        /// Get volume in effect
        /// </summary>
        /// <returns>Volume</returns>
        public float GetVolume()
        {
            return Volume;
        }

        /// <summary>
        /// Pause playback
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// Resume playback
        /// </summary>
        public abstract void Resume();
        /// <summary>
        /// Dispose of instance
        /// </summary>
        public abstract void Dispose();
    }
}
