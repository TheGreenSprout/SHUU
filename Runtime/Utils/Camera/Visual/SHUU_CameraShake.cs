using System.Collections.Generic;

namespace SHUU.UserSide.Addons.CameraShakeSystem
{
    public static class SHUU_CameraShake
    {
        #region Variables
        private static readonly List<CameraShake> Registered = new List<CameraShake>();

        public static IReadOnlyList<CameraShake> Shakes => Registered;
        #endregion




        #region Logic

        #region Registration
        internal static void Register(CameraShake shake)
        {
            if (!Registered.Contains(shake)) Registered.Add(shake);
        }

        internal static void Unregister(CameraShake shake) => Registered.Remove(shake);
        #endregion



        #region Shaking
        #region XML doc
        /// <summary>
        /// Adds trauma to every registered camera shake.
        /// </summary>
        #endregion
        public static void AddTrauma(float amount)
        {
            foreach (CameraShake shake in Registered)
                shake.AddTrauma(amount);
        }

        #region XML doc
        /// <summary>
        /// Starts a shake that fades out over the given duration on every registered camera shake.
        /// </summary>
        /// <param name="intensity">Trauma to start at (0-1).</param>
        /// <param name="duration">Seconds until it fades out.</param>
        #endregion
        public static void Shake(float intensity, float duration)
        {
            foreach (CameraShake shake in Registered)
                shake.Shake(intensity, duration);
        }

        public static void Stop()
        {
            foreach (CameraShake shake in Registered)
                shake.Stop();
        }
        #endregion

        #endregion
    }
}
