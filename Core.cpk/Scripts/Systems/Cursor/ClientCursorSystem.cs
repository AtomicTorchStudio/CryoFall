namespace AtomicTorch.CBND.CoreMod.Systems.Cursor
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ClientCursorSystem : ProtoSystem<ClientCursorSystem>
    {
        private static readonly IReadOnlyDictionary<CursorId, CursorResource> CursorResources =
            new Dictionary<CursorId, CursorResource>()
            {
                { CursorId.Default, new CursorResource("cursor") },
                { CursorId.InteractionPossible, new CursorResource("cursor_cog") },
                { CursorId.InteractionImpossible, new CursorResource("cursor_nocog") },
                { CursorId.PickupPossible, new CursorResource("cursor_hand") },
                { CursorId.PickupImpossible, new CursorResource("cursor_nohand") },
            };

        private static CursorId currentCursorId;

        public static CursorId CurrentCursorId
        {
            get => currentCursorId;
            set
            {
                if (currentCursorId == value)
                {
                    return;
                }

                currentCursorId = value;
                //Client.Rendering.CurrentCursor = GetCursorResource(value);
                //Logger.WriteDev("Cursor changed to: " + value);
            }
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                Client.Rendering.CurrentCursor = GetCursorResource(CursorId.Default);
                ClientUpdateHelper.UpdateCallback += ClientUpdate;
            }
        }

        private static void ClientUpdate()
        {
            ClientCursorWeaponSystem.Update();
            Client.Rendering.CurrentCursor = ClientCursorWeaponSystem.CurrentCursorResource
                                             ?? GetCursorResource(CurrentCursorId);
        }

        private static CursorResource GetCursorResource(CursorId cursorId)
        {
            return CursorResources[cursorId];
        }
    }
}