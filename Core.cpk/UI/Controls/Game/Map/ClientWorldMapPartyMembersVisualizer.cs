namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapPartyMembersVisualizer : IDisposable, IWorldMapVisualizer
    {
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
            PartyMembersVisualizationSystem.ClientIsEnabled = false;

            PartyMembersVisualizationSystem.ClientUpdateReceived
                -= this.PartyMembersVisualizationSystemClientUpdateReceivedHandler;

            PartySystem.ClientCurrentPartyMemberAddedOrRemoved
                -= this.PartySystemCurrentPartyMemberAddedOrRemovedHandler;

            this.RemoveAllMarkers();
        }

        private void AddMarker(string memberName, PartyMembersVisualizationSystem.NetworkPartyMemberData partyMemberData)
        {
            if (this.characterMarkers.ContainsKey(memberName))
            {
                return;
            }

            var mapControl = new WorldMapMarkPartyMember(memberName);
            this.UpdatePosition(mapControl, partyMemberData.Position);
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
                    this.UpdatePosition(control, entry.Position);
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
            PartyMembersVisualizationSystem.ClientIsEnabled = this.isEnabled;
            this.RemoveAllMarkers();

            if (!this.isEnabled)
            {
                return;
            }

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

        private void UpdatePosition(FrameworkElement mapControl, Vector2Ushort position)
        {
            var canvasPosition = this.worldMapController.WorldToCanvasPosition(position.ToVector2D());
            Canvas.SetLeft(mapControl, canvasPosition.X);
            Canvas.SetTop(mapControl, canvasPosition.Y);
        }
    }
}