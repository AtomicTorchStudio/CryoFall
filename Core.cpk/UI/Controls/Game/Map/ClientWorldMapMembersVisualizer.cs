namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.MembersMapVisualization;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapMembersVisualizer : BaseWorldMapVisualizer
    {
        private static readonly ICharactersClientService Characters = Api.Client.Characters;

        private readonly Dictionary<string, FrameworkElement> characterMarkers
            = new();

        public ClientWorldMapMembersVisualizer(WorldMapController worldMapController)
            : base(worldMapController)
        {
            MembersMapVisualizationSystem.ClientUpdateReceived
                += this.PartyMembersVisualizationSystemClientUpdateReceivedHandler;

            PartySystem.ClientCurrentPartyMemberAddedOrRemoved
                += this.CurrentPartyMemberAddedOrRemovedHandler;

            FactionSystem.ClientCurrentFactionMemberAddedOrRemoved
                += this.CurrentFactionMemberAddedOrRemovedHandler;

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

        protected override void DisposeInternal()
        {
            MembersMapVisualizationSystem.ClientDisableFor(this);

            MembersMapVisualizationSystem.ClientUpdateReceived
                -= this.PartyMembersVisualizationSystemClientUpdateReceivedHandler;

            PartySystem.ClientCurrentPartyMemberAddedOrRemoved
                -= this.CurrentPartyMemberAddedOrRemovedHandler;

            ClientUpdateHelper.UpdateCallback -= this.Update;

            this.RemoveAllMarkers();
        }

        protected override void OnDisable()
        {
            this.Reset();
        }

        protected override void OnEnable()
        {
            this.Reset();
        }

        private static bool IsMemberInScope(ICharacter character)
        {
            return character is not null
                   && character.IsInitialized
                   && character.TilePosition != Vector2Ushort.Zero
                   // if physics body is active but position is zero the player will stuck at the bottom left corner of the world
                   && character.TilePosition != Api.Client.World.WorldBounds.Offset + (1, 1);
        }

        private void AddMarker(
            string memberName,
            MembersMapVisualizationSystem.NetworkPartyMemberData partyMemberData)
        {
            if (this.characterMarkers.ContainsKey(memberName))
            {
                return;
            }

            var mapControl = new WorldMapMarkPartyMember(memberName);
            this.UpdatePosition(memberName, mapControl, partyMemberData.Position);
            Panel.SetZIndex(mapControl, 9);

            this.AddControl(mapControl);
            this.characterMarkers[memberName] = mapControl;
        }

        private void CurrentFactionMemberAddedOrRemovedHandler((FactionMemberEntry entry, bool isAdded) obj)
        {
            this.Reset();
        }

        private void CurrentPartyMemberAddedOrRemovedHandler((string name, bool isAdded) obj)
        {
            // party changed - rebuild map data completely
            this.Reset();
        }

        private void PartyMembersVisualizationSystemClientUpdateReceivedHandler(
            IReadOnlyList<MembersMapVisualizationSystem.ClientPartyMemberData> data)
        {
            foreach (var entry in data)
            {
                if (this.characterMarkers.TryGetValue(entry.Name, out var control))
                {
                    this.UpdatePosition(entry.Name, control, entry.Position);
                }
            }
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
            this.RemoveControl(control);
        }

        private void Reset()
        {
            this.RemoveAllMarkers();

            if (!this.IsEnabled)
            {
                MembersMapVisualizationSystem.ClientDisableFor(this);
                return;
            }

            MembersMapVisualizationSystem.ClientEnableFor(this);
            var currentCharacterName = ClientCurrentCharacterHelper.Character.Name;

            foreach (var partyMemberName in PartySystem.ClientGetCurrentPartyMembers())
            {
                if (partyMemberName == currentCharacterName)
                {
                    continue;
                }

                this.AddMarker(partyMemberName,
                               new MembersMapVisualizationSystem.NetworkPartyMemberData(
                                   Vector2Ushort.Zero));
            }

            var privateFactionMembers = FactionSystem.ClientCurrentFaction is not null
                                        && FactionSystem.ClientCurrentFactionKind != FactionKind.Public
                                            ? FactionSystem.ClientGetCurrentFactionMembers()
                                            : Array.Empty<FactionMemberEntry>();

            foreach (var factionMemberEntry in privateFactionMembers)
            {
                var memberName = factionMemberEntry.Name;
                if (memberName == currentCharacterName)
                {
                    continue;
                }

                this.AddMarker(memberName,
                               new MembersMapVisualizationSystem.NetworkPartyMemberData(
                                   Vector2Ushort.Zero));
            }
        }

        private void Update()
        {
            if (!this.IsEnabled)
            {
                return;
            }

            foreach (var pair in this.characterMarkers)
            {
                var playerName = pair.Key;
                var character = Characters.FindCharacter(playerName);

                if (IsMemberInScope(character))
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

            if (IsMemberInScope(character))
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
            var canvasPosition = this.WorldToCanvasPosition(position);
            Canvas.SetLeft(control, canvasPosition.X);
            Canvas.SetTop(control, canvasPosition.Y);
        }
    }
}