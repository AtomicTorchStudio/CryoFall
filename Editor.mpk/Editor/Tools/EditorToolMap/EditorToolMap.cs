namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolMap
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorToolMap : BaseEditorTool
    {
        private const string DefaultSuggestedMapName = "NewMap.map";

        private const string FileDialogMapFileFilter = "Map file (*.map)|*.map";

        private const string FileDialogSavegameFileFilter = "Save file (*.save)|*.save";

        private static string lastMapPath;

        private ActionCommandWithCondition commandClientSaveWorld;

        private ViewModelEditorToolMapSettings settingsViewModel;

        public override string Name => "Map new/save/load";

        public override int Order => 200;

        public static void ClientLoadSavegameQuick()
        {
            GetProtoEntity<EditorToolMap>().CallServer(_ => _.ServerRemote_LoadSavegameQuick());
        }

        public static async void ClientSaveSavegameQuick()
        {
            await GetProtoEntity<EditorToolMap>().CallServer(_ => _.ServerRemote_SaveSavegameQuick());
            NotificationSystem.ClientShowNotification("Game saved",
                                                      color: NotificationColor.Good);
        }

        public override BaseEditorActiveTool Activate(BaseEditorToolItem item)
        {
            this.ClientGetLastMapPathFromServer();

            // there are no brush or any other active tool
            return null;
        }

        public async void ClientSaveWorldLastFile()
        {
            if (lastMapPath is null)
            {
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            NotificationSystem.ClientShowNotification(
                $"Saving world to {Api.Shared.GetFileNameWithoutExtension(lastMapPath)}...",
                color: NotificationColor.Good);

            try
            {
                await this.CallServer(_ => _.ServerRemote_SaveWorld(lastMapPath));
            }
            catch
            {
                NotificationSystem.ClientShowNotification(
                    "World is not saved![br]Read the error in the server log",
                    color: NotificationColor.Bad);
                return;
            }
            finally
            {
                stopwatch.Stop();
            }

            NotificationSystem.ClientShowNotification(
                $"World saved successfully![br]It took: {stopwatch.Elapsed.TotalSeconds:0.#} sec.",
                color: NotificationColor.Good);
        }

        public override FrameworkElement CreateSettingsControl()
        {
            var control = new EditorToolMapSettings();
            if (this.settingsViewModel is null)
            {
                this.commandClientSaveWorld = new ActionCommandWithCondition(
                    this.ClientSaveWorldLastFile,
                    () => lastMapPath is not null);

                this.settingsViewModel = new ViewModelEditorToolMapSettings(
                    new ActionCommand(this.ClientNewWorld),
                    new ActionCommand(this.ClientOpenWorld),
                    new ActionCommand(this.ClientSaveWorldAs),
                    this.commandClientSaveWorld,
                    new ActionCommand(this.ClientLoadSavegame),
                    new ActionCommand(this.ClientSaveSavegameAs));
            }

            control.Setup(this.settingsViewModel);
            return control;
        }

        private async void ClientGetLastMapPathFromServer()
        {
            var mapPath = await this.CallServer(_ => _.ServerRemote_GetLastMapNameOrPath());
            lastMapPath = string.IsNullOrEmpty(mapPath) ? null : mapPath;
            this.commandClientSaveWorld.OnCanExecuteChanged();
        }

        private void ClientLoadSavegame()
        {
            Client.Core.EditorShowOpenFileDialog(
                FileDialogSavegameFileFilter,
                onPathSelected: filePath => { this.CallServer(_ => _.ServerRemote_LoadSavegame(filePath)); });
        }

        private void ClientNewWorld()
        {
            var dialog = new DialogCreateWorld();
            dialog.OkCallback =
                () => this.CallServer(
                    _ => _.ServerRemote_CreateWorld(dialog.ViewModel.CreateBounds()));

            Api.Client.UI.LayoutRootChildren.Add(dialog);
        }

        private void ClientOpenWorld()
        {
            Client.Core.EditorShowOpenFileDialog(
                FileDialogMapFileFilter,
                onPathSelected: filePath =>
                                {
                                    lastMapPath = filePath;
                                    this.CallServer(_ => _.ServerRemote_LoadWorld(filePath));
                                });
        }

        private void ClientSaveSavegameAs()
        {
            Client.Core.EditorShowSaveFileDialog(
                fileName: null,
                filter: FileDialogSavegameFileFilter,
                onPathSelected:
                async filePath =>
                {
                    await this.CallServer(_ => _.ServerRemote_SaveSavegame(filePath));

                    NotificationSystem.ClientShowNotification("Game saved",
                                                              color: NotificationColor.Good);
                });
        }

        private void ClientSaveWorldAs()
        {
            Client.Core.EditorShowSaveFileDialog(
                lastMapPath ?? DefaultSuggestedMapName,
                filter: FileDialogMapFileFilter,
                onPathSelected: filePath =>
                                {
                                    lastMapPath = filePath;
                                    this.commandClientSaveWorld.OnCanExecuteChanged();
                                    this.ClientSaveWorldLastFile();
                                });
        }

        private void ServerRemote_CreateWorld(BoundsUshort bounds)
        {
            lastMapPath = null;
            var water = GetProtoEntity<TileWaterSea>();
            Server.World.CreateWorld(water, bounds);
        }

        private string ServerRemote_GetLastMapNameOrPath()
        {
            return Server.World.LastMapPath;
        }

        private void ServerRemote_LoadSavegame(string filePath)
        {
            Logger.Important("Loading savegame from file path: " + filePath);
            Server.World.EditorLoadSavegame(filePath);
        }

        private void ServerRemote_LoadSavegameQuick()
        {
            Api.Server.World.EditorLoadSavegameQuick();
        }

        private void ServerRemote_LoadWorld(string filePath)
        {
            Logger.Important("Loading world from file path: " + filePath);
            Server.World.EditorLoadWorld(filePath);
        }

        private async Task<bool> ServerRemote_SaveSavegame(string filePath)
        {
            Logger.Important("Saving savegame to file path: " + filePath);
            await Server.World.EditorSaveSavegame(filePath);
            return true;
        }

        private async Task<bool> ServerRemote_SaveSavegameQuick()
        {
            await Api.Server.World.EditorSaveSavegameQuick();
            return true;
        }

        private bool ServerRemote_SaveWorld(string filePath)
        {
            Server.World.EditorSaveWorld(filePath);
            return true;
        }
    }
}