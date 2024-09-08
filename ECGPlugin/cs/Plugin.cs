using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

namespace SamplePlugin.Windows
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!; // Интерфейс плагина Dalamud
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!; // Провайдер текстур
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!; // Менеджер команд
        [PluginService] internal static IClientState ClientState { get; private set; } = null!; // Состояние клиента

        private const string CommandName = "/ECG"; // Команда для вызова плагина
        public Configuration Configuration { get; init; } // Конфигурация плагина
        public HeartRateMonitor HeartRateMonitor { get; private set; } // Монитор частоты сердечных сокращений
        public readonly WindowSystem WindowSystem = new("ECG Plugin"); // Система окон для плагина
        public MainWindow MainWindow { get; private set; } // Главное окно плагина
        public ConfigWindow ConfigWindow { get; private set; } // Окно конфигурации плагина

        public Plugin()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration(); // Загрузка конфигурации или создание новой
            HeartRateMonitor = new HeartRateMonitor(Configuration); // Инициализация монитора частоты сердечных сокращений

            var imagePaths = new string[20]; // Массив путей к изображениям
            for (var i = 0; i < 20; i++)
            {
                imagePaths[i] = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, $"{i}.png"); // Определение пути к изображению
            }

            ConfigWindow = new ConfigWindow(this); // Создание окна конфигурации
            MainWindow = new MainWindow(this, imagePaths); // Создание главного окна
            WindowSystem.AddWindow(ConfigWindow); // Добавляем окно конфигурации в систему окон
            WindowSystem.AddWindow(MainWindow); // Добавляем главное окно в систему окон

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open ECG window" // Сообщение помощи для команды
            });

            PluginInterface.UiBuilder.Draw += DrawUI; // Отрисовка окон
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI; // Открытие окна конфигурации
            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI; // Открытие главного окна
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows(); // Удаление всех окон из системы окон
            ConfigWindow.Dispose(); // Освобождение ресурсов окон
            MainWindow.Dispose(); // Освобождение ресурсов окон
            CommandManager.RemoveHandler(CommandName); // Удаление обработчика команды
        }

        private void OnCommand(string command, string args) => ToggleMainUI(); // Переключение главного окна

        private void DrawUI() => WindowSystem.Draw(); // Отрисовка окон плагина

        public void ToggleConfigUI() => ConfigWindow.Toggle(); // Переключение окна конфигурации
        public void ToggleMainUI() => MainWindow.Toggle(); // Переключение главного окна
    }
}
