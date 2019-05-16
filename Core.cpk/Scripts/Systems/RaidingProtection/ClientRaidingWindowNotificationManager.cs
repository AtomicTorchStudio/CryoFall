namespace AtomicTorch.CBND.CoreMod.Systems.RaidingProtection
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientRaidingWindowNotificationManager
    {
        private static bool isInitialized;

        private static bool? wasOpen;

        public static void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            RaidingProtectionSystem.ClientRaidingWindowChanged += ClientRaidingWindowChangedHandler;
            UpdateNextRaidingInfo();
        }

        private static void ClientRaidingWindowChangedHandler()
        {
            wasOpen = null;
        }

        private static void UpdateNextRaidingInfo()
        {
            try
            {
                if (RaidingProtectionSystem.ClientIsRaidingWindowEnabled)
                {
                    var hoursTillNextRaid = RaidingProtectionSystem.SharedCalculateTimeUntilNextRaid()
                                                                   .TotalHours;
                    if (hoursTillNextRaid <= 0)
                    {
                        // raiding window now
                        if (!wasOpen.HasValue
                            || !wasOpen.Value)
                        {
                            // was unknown or closed, now opened - notify
                            RaidingProtectionSystem.ClientShowNotificationRaidingWindowNowOpened();
                        }

                        wasOpen = true;
                    }
                    else
                    {
                        // raiding window later
                        if (wasOpen.HasValue
                            && wasOpen.Value)
                        {
                            // was opened, now closed - notify
                            RaidingProtectionSystem.ClientShowNotificationRaidingWindowNowClosed();
                        }

                        wasOpen = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Api.Logger.Exception(ex);
            }
            finally
            {
                // schedule next update
                ClientComponentTimersManager.AddAction(
                    delaySeconds: 1,
                    UpdateNextRaidingInfo);
            }
        }
    }
}