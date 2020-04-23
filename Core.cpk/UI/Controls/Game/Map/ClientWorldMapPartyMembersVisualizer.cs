namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapPartyMembersVisualizer : IWorldMapVisualizer
    {
        private static readonly ICharactersClientService Characters = Api.Client.Characters;

        private readonly Dictionary<string, FrameworkElement> characterMarkers
            = new Dictionary<string, FrameworkElement>();

        private readonly WorldMapController worldMapController;

        private bool isEnabled;

        public ClientWorldMapPartyMembersVisualizer(WorldMapController worldMapController)
        {
            this.worldMapController = worldMapController;

            PartyMembersVisualizationSystem.ClientUpdateReceived
                += this.PartyMembersVisualizationSystemClientUpdateReceivedHandler;

            PartySystem.ClientCurrentPartyMemberAddedOrRemoved
                += this.PartySystemCurrentPartyMemberAddedOrRemovedHandler;

            ClientUpdateHelper.UpdateCallback += this.Update;
        }

        public static Vector2Ushort ClientGraveyardPosition
        {
            get
            {
                var world = Api.Client.World;
                var worldBounds = world.WorldBounds;

                // bottom right corner of the map
                return new Vector2Ushort((ushort)(worldBounds.Offset.X + worldBounds.Size.X - 1),
                                         (ushort)(worldBounds.Offset.Y + 1));
            }
        }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }

                this.isEnabled = value;
                this.Reset();
            }
        }

        public void Dispose()
        {
            PartyMembersVisualizationSystem.ClientDisableFor(this);

            PartyMembersVisualizationSystem.ClientUpdateReceived
                -= this.PartyMembersVisualizationSystemClientUpdateReceivedHandler;

            PartySystem.ClientCurrentPartyMemberAddedOrRemoved
                -= this.PartySystemCurrentPartyMemberAddedOrRemovedHandler;

            ClientUpdateHelper.UpdateCallback -= this.Update;

            this.RemoveAllMarkers();
        }

        private static bool IsPartyMemberInScope(ICharacter character)
        {
            return !(character is null)
                   && character.IsInitialized
                   && character.TilePosition != Vector2Ushort.Zero
                   // if physics body is active but position is zero the player will stuck at the bottom left corner of the world
                   && character.TilePosition != Api.Client.World.WorldBounds.Offset + (1, 1);
        }

        private void AddMarker(
            string memberName,
            PartyMembersVisualizationSystem.NetworkPartyMemberData partyMemberData)
        {
            if (this.characterMarkers.ContainsKey(memberName))
            {
                return;
            }

            var mapControl = new WorldMapMarkPartyMember(memberName);
            this.UpdatePosition(memberName, mapControl, partyMemberData.Position);
            Panel.SetZIndex(mapControl, 9);

            this.worldMapController.AddControl(mapControl);
            this.characterMarkers[memberName] = mapControl;
        }

        private void PartyMembersVisualizationSystemClientUpdateReceivedHandler(
            IReadOnlyList<PartyMembersVisualizationSystem.ClientPartyMemberData> data)
        {
            foreach (var entry in data)
            {
                if (this.characterMarkers.TryGetValue(entry.Name, out var control))
                {
                    this.UpdatePosition(entry.Name, control, entry.Position);
                }
            }
        }

        private void PartySystemCurrentPartyMemberAddedOrRemovedHandler((string name, bool isAdded) obj)
        {
            // party changed - rebuild map data completely
            this.Reset();
        }

        private void RemoveAllMarkers()
        {
            if (this.characterMarkers.Count != 0)
            {
                foreach (var mark in this.characterMarkers.Keys.ToList())
                {
                    this.RemoveMarker(mark);
                }
            }
        }

        private void RemoveMarker(string memberName)
        {
            if (!this.characterMarkers.TryGetValue(memberName, out var control))
            {
                return;
            }

            this.characterMarkers.Remove(memberName);
            this.worldMapController.RemoveControl(control);
        }

        private void Reset()
        {
            this.RemoveAllMarkers();

            if (!this.isEnabled)
            {
                PartyMembersVisualizationSystem.ClientDisableFor(this);
                return;
            }

            PartyMembersVisualizationSystem.ClientEnableFor(this);
            var currentCharacterName = ClientCurrentCharacterHelper.Character.Name;

            foreach (var memberName in PartySystem.ClientGetCurrentPartyMembers())
            {
                if (memberName == currentCharacterName)
                {
                    continue;
                }

                this.AddMarker(memberName,
                               new PartyMembersVisualizationSystem.NetworkPartyMemberData(
                                   Vector2Ushort.Zero));
            }
        }

        private void Update()
        {
            if (!this.isEnabled)
            {
                return;
            }

            foreach (var pair in this.characterMarkers)
            {
                var playerName = pair.Key;
                var character = Characters.FindCharacter(playerName);

                if (IsPartyMemberInScope(character))
                {
                    // have this character in scope, update its latest position
                    var control = pair.Value;
                    this.UpdatePosition(control, character.Position);
                }
            }
        }

        private void UpdatePosition(string playerName, FrameworkElement control, in Vector2Ushort tilePosition)
        {
            Vector2D position;
            var character = Characters.FindCharacter(playerName);

            if (IsPartyMemberInScope(character))
            {
                // use actual position
                position = character.Position;
            }
            else
            {
                position = (tilePosition.X + 0.5, tilePosition.Y + 0.5);
            }

            this.UpdatePosition(control, position);
        }

        private void UpdatePosition(FrameworkElement control, in Vector2D position)
        {
            if (position.ToVector2Ushort() == ClientGraveyardPosition)
            {
                // player character is dead or despawned
                control.Visibility = Visibility.Collapsed;
                return;
            }

            control.Visibility = Visibility.Visible;
            var canvasPosition = this.worldMapController.WorldToCanvasPosition(position);
            Canvas.SetLeft(control, canvasPosition.X);
            Canvas.SetTop(control, canvasPosition.Y);
        }
    }
}