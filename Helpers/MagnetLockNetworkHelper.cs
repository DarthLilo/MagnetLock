using MagnetLock.Patches;
using Unity.Netcode;

namespace MagnetLock.Helpers
{
    internal class MagnetLockNetworkHelper : NetworkBehaviour
    {
        public static MagnetLockNetworkHelper Instance { get; private set; }

        private void Start()
        {
            Instance = this;
            MagnetLock.Logger.LogInfo("MagnetLockNetworkHelper.Start() initialized!");
        }

        [ClientRpc]
        public void UpdateMagnetStateClientRpc(bool state)
        {
            if (PatchMagnet.magnetLeverTrigger != null)
            {
                if (state == true)
                {
                    PatchMagnet.magnetLeverTrigger.interactable = true;
                    PatchMagnet.magnetLeverTrigger.hoverIcon = PatchMagnet.defaultHoverIcon;
                    PatchMagnet.magnetLeverTrigger.disabledHoverIcon = PatchMagnet.defaultDisabledHoverIcon;
                }
                else
                {
                    PatchMagnet.magnetLeverTrigger.interactable = false;
                    PatchMagnet.magnetLeverTrigger.hoverIcon = null;
                    PatchMagnet.magnetLeverTrigger.disabledHoverIcon = null;
                }
            }
        }
    }
}