using Dalamud.Configuration;
using System;

namespace SamplePlugin.Windows
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0; // Версия конфигурации плагина

        public bool IsConfigWindowMovable { get; set; } = true; // Опция для перемещения окна конфигурации

        public bool ShowHealthPercentage { get; set; } = true; // Отображение процента здоровья

        public bool MaxHeartRateLimitEnabled { get; set; } = false; // Включение/отключение ограничения максимального пульса
        public bool MinHeartRateLimitEnabled { get; set; } = false; // Включение/отключение ограничения минимального пульса

        public int UpdateFrequency { get; set; } = 1; // Частота обновления данных

        public float MaxHeartRate { get; set; } = 180f; // Максимальный пульс
        public float MinHeartRate { get; set; } = 60f; // Минимальный пульс

        public int HeartRateDataSize { get; set; } = 135; // Размер данных пульса

        public float SpikeHeight { get; set; } = 1.4f; // Высота пика
        public float SmallSpikeHeight { get; set; } = -0.3f; // Высота маленького пика
        public int SpikeInterval { get; set; } = 50; // Интервал пиков

        public bool IsImageTransparencyEnabled { get; set; } = false; // Поле для включения/отключения прозрачности
        public float ImageTransparency { get; set; } = 1.0f; // Прозрачность по умолчанию

        public float SpikeIntervalAt100Percent { get; set; } = 19.3f; // Интервал пика при 100% здоровья
        public float SpikeIntervalAt1Percent { get; set; } = 7.0f; // Интервал пика при 1% здоровья
        public float SpikeIntervalAt0Percent { get; set; } = 1.0f; // Интервал пика при 0% здоровья

        public float MaxUpdateInterval { get; set; } = 0.010f; // Максимальный интервал обновления
        public float MinUpdateInterval { get; set; } = 0.010f; // Минимальный интервал обновления

        public float MinBeatingSpeed { get; set; } = 6f; // Минимальная скорость биения
        public float MaxBeatingSpeed { get; set; } = 42f; // Максимальная скорость биения

        public float MinPauseBetweenBeats { get; set; } = 0f; // Минимальная пауза между биениями
        public float MaxPauseBetweenBeats { get; set; } = 140f; // Максимальная пауза между биениями

        public string CurrentImage { get; set; } = "0.png"; // Текущий путь к изображению
        public string[] ImagePaths { get; set; } = new string[20]; // Массив путей к изображениям

        public int AnimationSpeedAt100Percent { get; set; } = 41; // Скорость анимации при 100% здоровья
        public int AnimationSpeedAt1Percent { get; set; } = 140; // Скорость анимации при 1% здоровья
        public float AnimationSpeedMs { get; set; } = 41f; // Скорость анимации в миллисекундах
        public float PauseLengthMs { get; set; } = 140f; // Длина паузы в миллисекундах

        public int pauseLengthMs { get; internal set; } // Внутреннее поле для длины паузы

        public SimulationMode SimulationMode { get; set; } = SimulationMode.None; // Режим симуляции

        public float ImageSize { get; set; } = 3.5f; // Размер изображения
        public float ECGWidth { get; set; } = 375f; // Ширина ЭКГ

        public float SimulatedHealthPercentage { get; internal set; } = 100; // Процент здоровья в симуляции

        public object? AnimationSpeedAt0Percent { get; internal set; } // Скорость анимации при 0% здоровья
        public int PauseLengthAt1Percent { get; internal set; } // Длина паузы при 1% здоровья
        public int PauseLengthAt100Percent { get; internal set; } // Длина паузы при 100% здоровья

        public void Save() => Plugin.PluginInterface.SavePluginConfig(this); // Метод для сохранения конфигурации плагина
    }

    public enum SimulationMode
    {
        None,             // Нет симуляции
        Simulate100HP,    // Симуляция 100% здоровья
        Simulate10HP,     // Симуляция 10% здоровья
        SimulateCustomHP  // Симуляция с пользовательским уровнем здоровья
    }
}
