using System;
using UnityEngine.Events;

namespace SHUU.UserSide.Addons.SavingSystem
{
    #region XML doc
    /// <summary>
    /// The text of a save file (or of its info) on its way to or from the disk. Listeners of the SavingManager's data hooks change "text" to transform it
    /// (encrypt, compress, encode...). A listener that doesn't touch it leaves the data as it is.
    /// </summary>
    #endregion
    public sealed class SavePayload
    {
        #region Variables
        private string _text;
        public string text
        {
            get => _text;

            set
            {
                _text = value;
                modified = true;
            }
        }


        internal bool modified;


        public string saveId { get; }
        #endregion




        #region Main
        internal SavePayload(string saveId, string text)
        {
            this.saveId = saveId;
            _text = text;
        }
        #endregion
    }





    #region XML doc
    /// <summary>
    /// UnityEvent that carries a SavePayload (assign methods with a SavePayload parameter in the inspector, under "Dynamic").
    /// </summary>
    #endregion
    [Serializable]
    public class SavePayloadEvent : UnityEvent<SavePayload> { }
}
